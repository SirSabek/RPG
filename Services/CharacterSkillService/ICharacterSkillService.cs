using RPG.Dtos.Character;
using RPG.Dtos.CharacterSkill;
using RPG.Models;

namespace RPG.Services.CharacterSkillService;

public interface ICharacterSkillService
{
    Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill);
}