using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RPG.Data;
using RPG.Dtos.Fight;
using RPG.Models;

namespace RPG.Services.FightService;

public class FightService : IFightService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public FightService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
    {
        var response = new ServiceResponse<AttackResultDto>();
        try
        {
            var attacker = await _context.Characters
                .Include(c => c.Weapon)
                .FirstOrDefaultAsync(c => c.Id == request.AttackerId && c.User.Id == int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
            var opponent = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

            var damage = DoWeaponAttack(attacker, opponent);
            if (opponent.HitPoints <= 0)
            {
                response.Message = $"{opponent.Name} has been defeated!";
            }
            _context.Characters.Update(opponent);
            await _context.SaveChangesAsync();

            response.Data = new AttackResultDto
            {
                Attacker = attacker.Name,
                AttackerHP = attacker.HitPoints,
                Opponent = opponent.Name,
                OpponentHP = opponent.HitPoints,
                Damage = damage
            };
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
    {
        var response = new ServiceResponse<AttackResultDto>();
        try
        {
            var attacker = await _context.Characters
                .Include(c => c.CharacterSkills).ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.Id == request.AttackerId && c.User.Id == int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
            var opponent = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == request.OpponentId);
            var skill = attacker.CharacterSkills.FirstOrDefault(cs => cs.Skill.Id == request.SkillId);
            if (skill == null)
            {
                response.Success = false;
                response.Message = $"{attacker.Name} doesn't know that skill.";
                return response;
            }

            var damage = DoSkillAttack(attacker, opponent, skill);
            if (opponent.HitPoints <= 0)
            {
                response.Message = $"{opponent.Name} has been defeated!";
            }
            _context.Characters.Update(opponent);
            await _context.SaveChangesAsync();

            response.Data = new AttackResultDto
            {
                Attacker = attacker.Name,
                AttackerHP = attacker.HitPoints,
                Opponent = opponent.Name,
                OpponentHP = opponent.HitPoints,
                Damage = damage
            };
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
    {
        var response = new ServiceResponse<FightResultDto>
        {
            Data = new FightResultDto()
        };
        try
        {
            var characters = await _context.Characters
                .Include(c => c.Weapon)
                .Include(c => c.CharacterSkills).ThenInclude(cs => cs.Skill)
                .Where(c => request.CharacterIds.Contains(c.Id)).ToListAsync();
            var defeated = false;
            while (!defeated)
            {
                foreach (var attacker in characters)
                {
                    var opponent = characters.FirstOrDefault(c => c.Id != attacker.Id);
                    var damage = DoWeaponAttack(attacker, opponent);
                    if (opponent.HitPoints <= 0)
                    {
                        defeated = true;
                        attacker.Victories++;
                        opponent.Defeats++;
                        response.Data.Log.Add($"{attacker.Name} defeated {opponent.Name}");
                        break;
                    }
                    response.Data.Log.Add($"{attacker.Name} attacks {opponent.Name} for {damage} damage.");
                }
            }

            characters.ForEach(c =>
            {
                c.Fights++;
                c.HitPoints = 100;
            });
            _context.Characters.UpdateRange(characters);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<ServiceResponse<List<HighScoreDto>>> GetHighScore()
    {
        var response = new ServiceResponse<List<HighScoreDto>>();
        try
        {
            var characters = await _context.Characters
                .Include(c => c.User)
                .OrderByDescending(c => c.Victories).ThenBy(c => c.Defeats)
                .Take(10)
                .Select(c => _mapper.Map<HighScoreDto>(c))
                .ToListAsync();
            response.Data = characters;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }
        return response;
    }

    private static int DoWeaponAttack(Character attacker, Character opponent)
    {
        var damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
        damage -= new Random().Next(opponent.Defense);
        if (damage > 0)
        {
            opponent.HitPoints -= damage;
        }
        return damage;
    }
    
    private static int DoSkillAttack(Character attacker, Character opponent, CharacterSkill characterSkill)
    {
        var damage = characterSkill.Skill.Damage + (new Random().Next(attacker.Intelligence));
        damage -= new Random().Next(opponent.Defense);
        if (damage > 0)
        {
            opponent.HitPoints -= damage;
        }
        return damage;
    }
}