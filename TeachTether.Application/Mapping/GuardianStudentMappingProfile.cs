using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping;

public class GuardianStudentMappingProfile : Profile
{
    public GuardianStudentMappingProfile()
    {
        CreateMap<CreateStudentGuardianRequest, GuardianStudent>();
        CreateMap<CreateGuardianStudentRequest, GuardianStudent>();
    }
}