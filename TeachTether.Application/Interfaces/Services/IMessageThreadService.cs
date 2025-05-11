using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IMessageThreadService
    {
        Task<MessageThreadResponse> GetByIdAsync(int id);
        Task<MessageThreadResponse> CreateAsync(CreateMessageThreadRequest request);
        Task<IEnumerable<MessageThreadResponse>> GetAllForUserAsync(string userId);
        Task DeleteAsync(int id);
    }
}
