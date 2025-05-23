using AutoMapper;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IMessageAttachmentService _messageAttachmentService;
        private readonly IMessageDeletionHelper _messageDeletionHelper;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IMessageAttachmentService messageAttachmentService, IMessageDeletionHelper messageDeletionHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _messageAttachmentService = messageAttachmentService;
            _messageDeletionHelper = messageDeletionHelper;
        }

        public async Task<MessageResponse> CreateAsync(CreateMessageRequest request, int threadId, string userId)
        {
            var message = _mapper.Map<Message>(request);
            message.SentAt = DateTime.Now;
            message.SenderUserId = userId;
            message.ThreadId = threadId;
            message.IsRead = false;

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            var attachments = new List<MessageAttachmentResponse>();

            foreach (var file in request.Attachments)
            {
                var attachment = await _messageAttachmentService.CreateAsync(message.Id, file);
                attachments.Add(attachment);
            }

            var response = _mapper.Map<MessageResponse>(message);
            response.Attachments = attachments;
            return response;
        }

        public async Task DeleteAsync(int id)
        {
            await _messageDeletionHelper.DeleteMessageAsync(id);
        }

        public async Task<IEnumerable<MessageResponse>> GetAllAsync(int threadId)
        {
            var messages = await _unitOfWork.Messages.GetAllAsync(m => m.ThreadId == threadId);
            var response = _mapper.Map<IEnumerable<MessageResponse>>(messages);
            foreach (var item in response)
            {
                var attachments = await _unitOfWork.MessageAttachments.GetAllAsync(ma => ma.MessageId == item.Id);
                item.Attachments = _mapper.Map<List<MessageAttachmentResponse>>(attachments);
            }
            return response;
        }

        public async Task<MessageResponse> GetByIdAsync(int id)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id)
                ?? throw new NotFoundException("message not found");

            var attachments = await _unitOfWork.MessageAttachments.GetAllAsync(ma => ma.MessageId == id);

            var response = _mapper.Map<MessageResponse>(message);
            response.Attachments = _mapper.Map<List<MessageAttachmentResponse>>(attachments);
            return response;
        }

        public async Task ReadAsync(int id)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id)
                ?? throw new NotFoundException("message not found");

            message.IsRead = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
