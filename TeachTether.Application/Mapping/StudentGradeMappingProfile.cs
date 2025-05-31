using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping;

public class StudentGradeMappingProfile : Profile
{
    public StudentGradeMappingProfile()
    {
        CreateMap<CreateStudentGradeRequest, StudentGrade>()
            .ForMember(dest => dest.GradeType,
                opt => opt.MapFrom(src => Enum.Parse<GradeType>(src.GradeType, true)));

        CreateMap<UpdateStudentGradeRequest, StudentGrade>()
            .ForMember(dest => dest.GradeType,
                opt => opt.MapFrom(src => Enum.Parse<GradeType>(src.GradeType, true)));

        CreateMap<StudentGrade, StudentGradeResponse>()
            .ForMember(dest => dest.GradeType,
                opt => opt.MapFrom(src => src.GradeType.ToString()));
    }
}