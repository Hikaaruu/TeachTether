using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class StudentAttendanceMappingProfile : Profile
    {
        public StudentAttendanceMappingProfile()
        {
            CreateMap<CreateStudentAttendanceRequest, StudentAttendance>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<AttendanceStatus>(src.Status, true)));

            CreateMap<UpdateStudentAttendanceRequest, StudentAttendance>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<AttendanceStatus>(src.Status, true)));

            CreateMap<StudentAttendance, StudentAttendanceResponse>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
