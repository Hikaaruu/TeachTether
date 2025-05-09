using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class ClassAssignmentMappingProfile : Profile
    {
        public ClassAssignmentMappingProfile()
        {
            CreateMap<CreateClassAssignmentRequest, ClassAssignment>();
        }
    }
}
