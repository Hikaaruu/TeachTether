using Microsoft.AspNetCore.Http;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IMessageAttachmentService
{
    Task<FileDownloadModel> GetFileByIdAsync(int id);
    Task<MessageAttachmentResponse> CreateAsync(int messageId, IFormFile file, CancellationToken ct = default);
}