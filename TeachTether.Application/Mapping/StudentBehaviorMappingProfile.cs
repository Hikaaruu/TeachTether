using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class StudentBehaviorMappingProfile : Profile
    {
        public StudentBehaviorMappingProfile()
        {
            CreateMap<CreateStudentBehaviorRequest, StudentBehavior>();
            CreateMap<UpdateStudentBehaviorRequest, StudentBehavior>();
            CreateMap<StudentBehavior, StudentBehaviorResponse>();
        }
    }
}
