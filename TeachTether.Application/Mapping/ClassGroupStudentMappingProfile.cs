using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class ClassGroupStudentMappingProfile : Profile
    {
        public ClassGroupStudentMappingProfile()
        {
            CreateMap<CreateClassGroupStudentRequest, ClassGroupStudent>();
        }
    }
}
