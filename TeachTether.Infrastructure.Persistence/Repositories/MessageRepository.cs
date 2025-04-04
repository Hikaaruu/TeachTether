using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetBySenderIdAsync(string senderId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.SenderUserId == senderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByThreadIdAsync(int threadId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.ThreadId == threadId)
                .ToListAsync();
        }
    }
}
