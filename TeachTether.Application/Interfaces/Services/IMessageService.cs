using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IMessageService
{
    Task<MessageResponse> GetByIdAsync(int id);
    Task<MessageResponse> CreateAsync(CreateMessageRequest request, int threadId, string userId);
    Task<IEnumerable<MessageResponse>> GetAllAsync(int threadId);
    Task<IEnumerable<MessageResponse>> GetByThreadAsync(int threadId, int take, int? beforeId);
    Task DeleteAsync(int id);
    Task ReadAsync(int id);
}