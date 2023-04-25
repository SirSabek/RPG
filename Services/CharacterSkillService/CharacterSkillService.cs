using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RPG.Data;
using RPG.Dtos.Character;
using RPG.Dtos.CharacterSkill;
using RPG.Models;

namespace RPG.Services.CharacterSkillService;

public class CharacterSkillService : ICharacterSkillService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _contextAccessor;
    
    public CharacterSkillService(IMapper mapper, DataContext context, IHttpContextAccessor contextAccessor)
    {
        _mapper = mapper;
        _context = context;
        _contextAccessor = contextAccessor;
    }
    public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
    {
        var response = new ServiceResponse<GetCharacterDto>();
        try
        {
            var character = await _context.Characters
                .Include(c=>c.Weapon)
                .Include(c=>c.CharacterSkills)
                .ThenInclude(c=>c.Skill)
                .FirstOrDefaultAsync(c=>c.Id == newCharacterSkill.CharacterId &&
                                        c.User.Id == int.Parse(_contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (character == null)
            {
                response.Success = false;
                response.Message = "Character not found.";
                return response;
            }

            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);
            if (skill is null)
            {
                response.Success = false;
                response.Message = "Skill Not found";
                return response;
            }

            var characterSkill = new CharacterSkill
            {
                Character = character,
                Skill = skill
            };

            await _context.CharacterSkills.AddAsync(characterSkill);
            await _context.SaveChangesAsync();

            response.Data = _mapper.Map<GetCharacterDto>(character);

        }
        catch (Exception e)
        {
            response.Success = false;
            response.Message = e.Message;
        }

        return response;
    }
}