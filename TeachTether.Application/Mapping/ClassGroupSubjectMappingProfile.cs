using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class ClassGroupSubjectMappingProfile : Profile
    {
        public ClassGroupSubjectMappingProfile()
        {
            CreateMap<CreateClassGroupSubjectRequest,ClassGroupSubject>();
        }
    }
}
