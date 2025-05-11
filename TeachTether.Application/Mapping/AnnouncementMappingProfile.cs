using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping
{
    public class AnnouncementMappingProfile : Profile
    {
        public AnnouncementMappingProfile()
        {
            CreateMap<CreateAnnouncementRequest, Announcement>()
               .ForMember(dest => dest.TargetAudience,
                          opt => opt.MapFrom(src => Enum.Parse<AudienceType>(src.TargetAudience, true)));

            CreateMap<UpdateAnnouncementRequest, Announcement>();

            CreateMap<Announcement, AnnouncementResponse>();
        }
    }
}
