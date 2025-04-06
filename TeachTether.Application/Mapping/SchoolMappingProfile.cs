using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class SchoolMappingProfile : Profile
    {
        public SchoolMappingProfile()
        {
            CreateMap<CreateSchoolRequest, School>();
            CreateMap<UpdateSchoolRequest, School>();
            CreateMap<School, SchoolResponse>();
        }
    }
}
