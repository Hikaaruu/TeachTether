using AutoMapper;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src =>
                    src.Sex == 'M' ? Sex.M : Sex.F));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src =>
                    src.Sex == 'M' ? Sex.M : Sex.F));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src =>
                    src.Sex == 'M' ? Sex.M : Sex.F));

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => 
                    src.Sex.ToString()));
        }
    }
}
