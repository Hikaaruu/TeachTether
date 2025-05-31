using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping;

public class MessageMappingProfile : Profile
{
    public MessageMappingProfile()
    {
        CreateMap<CreateMessageRequest, Message>();
        CreateMap<Message, MessageResponse>();
    }
}