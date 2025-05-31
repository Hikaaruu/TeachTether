using AutoMapper;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Mapping;

public class MessageThreadMappingProfile : Profile
{
    public MessageThreadMappingProfile()
    {
        CreateMap<CreateMessageThreadRequest, MessageThread>();
        CreateMap<MessageThread, MessageThreadResponse>();
    }
}