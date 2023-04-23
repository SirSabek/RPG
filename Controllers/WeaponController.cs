using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RPG.Dtos.Weapon;
using RPG.Services.WeaponService;

namespace RPG.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeaponController : ControllerBase
    {
        private IWeaponService _weaponService;
        
        public WeaponController(IWeaponService weaponService)
        {
            _weaponService = weaponService;
        }
        
        [HttpPost]
        public async Task<IActionResult> AddWeapon(AddWeaponDto newWeapon)
        {
            return Ok(await _weaponService.AddWeapon(newWeapon));
        }

    }
}