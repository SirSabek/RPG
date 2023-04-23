using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RPG.Data;
using RPG.Dtos.Character;
using RPG.Dtos.Weapon;
using RPG.Models;

namespace RPG.Services.WeaponService;

public class WeaponService : IWeaponService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _contextAccessor;
    
    public WeaponService(DataContext context, IMapper mapper, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _contextAccessor = contextAccessor;
    }
    public async Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon)
    {
        var response = new ServiceResponse<GetCharacterDto>();
        try
        {
            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == newWeapon.CharacterId &&
                                          c.User.Id == int.Parse(_contextAccessor.HttpContext.User
                                              .FindFirstValue(ClaimTypes.NameIdentifier)));
            if (character == null)
            {
                response.Success = false;
                response.Message = "Character not found.";
                return response;
            }

            var weapon = new Weapon
            {
                Name = newWeapon.Name,
                Damage = newWeapon.Damage,
                Character = character
            };
            await _context.Weapons.AddAsync(weapon);
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