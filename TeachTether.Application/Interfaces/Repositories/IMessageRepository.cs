using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetByThreadIdAsync(int threadId);
        Task<IEnumerable<Message>> GetByThreadIdAsync(int threadId, int take, int? beforeId = null);
        Task<IEnumerable<Message>> GetBySenderIdAsync(string senderId);
    }
}
