using AutoMapper;
using Microsoft.AspNetCore.Http;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class MessageAttachmentService : IMessageAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMessageRepository _messageRepository;

        public MessageAttachmentService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, IMessageRepository messageRepository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<MessageAttachmentResponse> CreateAsync(int messageId, IFormFile file, CancellationToken ct = default)
        {
            var message = await _messageRepository.GetByIdAsync(messageId)
                ?? throw new NotFoundException("Message not found");

            string relativePath = await _fileStorageService.UploadAsync(file, message.ThreadId, ct);

            var attachment = new MessageAttachment
            {
                MessageId = messageId,
                FileName = file.FileName,
                FileType = file.ContentType,
                FileSizeBytes = (int)file.Length,
                FileUrl = relativePath,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.MessageAttachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<MessageAttachmentResponse>(attachment);
        }

        public async Task<FileDownloadModel> GetFileByIdAsync(int id)
        {
            var attachment = await _unitOfWork.MessageAttachments.GetByIdAsync(id)
                ?? throw new NotFoundException("Attachment not found");

            var content = await _fileStorageService.DownloadAsync(attachment.FileUrl);

            return new FileDownloadModel(content, attachment.FileType, attachment.FileName);
        }
    }
}
