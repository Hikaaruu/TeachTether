using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class MessageRepository(ApplicationDbContext context) : Repository<Message>(context), IMessageRepository
{
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

    public async Task<IEnumerable<Message>> GetByThreadIdAsync(int threadId, int take, int? beforeId = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(m => m.ThreadId == threadId);

        if (beforeId.HasValue) query = query.Where(m => m.Id < beforeId.Value);

        return await query
            .OrderByDescending(m => m.Id)
            .Take(take)
            .ToListAsync();
    }
}