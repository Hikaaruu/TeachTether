using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping;

public class MessageAttachmentMappingProfile : Profile
{
    public MessageAttachmentMappingProfile()
    {
        CreateMap<MessageAttachment, MessageAttachmentResponse>();
    }
}