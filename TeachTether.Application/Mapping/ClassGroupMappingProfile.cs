using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping;

public class ClassGroupMappingProfile : Profile
{
    public ClassGroupMappingProfile()
    {
        CreateMap<CreateClassGroupRequest, ClassGroup>();
        CreateMap<UpdateClassGroupRequest, ClassGroup>();
        CreateMap<ClassGroup, ClassGroupResponse>();
    }
}