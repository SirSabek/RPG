﻿using AutoMapper;
using RPG.Dtos.Character;
using RPG.Dtos.Fight;
using RPG.Dtos.Skill;
using RPG.Dtos.Weapon;
using RPG.Models;

namespace RPG
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>()
                .ForMember(dto => dto.Skills, c=> c.MapFrom(c=>c.CharacterSkills.Select(cs=>cs.Skill)));
            CreateMap<AddCharacterDto, Character>();
            CreateMap<Weapon, GetWeaponDto>();
            CreateMap<Skill, GetSkillDto>();
            CreateMap<Character, HighScoreDto>();
        }
    }
}
