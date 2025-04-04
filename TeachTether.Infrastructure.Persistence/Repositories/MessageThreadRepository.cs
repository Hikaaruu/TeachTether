using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class MessageThreadRepository : Repository<MessageThread>, IMessageThreadRepository
    {
        public MessageThreadRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<MessageThread>> GetByGuardianIdAsync(int guardianId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(mt => mt.GuardianId == guardianId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MessageThread>> GetByTeacherIdAsync(int teacherId)
        {
            return await _dbSet
               .AsNoTracking()
               .Where(mt => mt.TeacherId == teacherId)
               .ToListAsync();
        }
    }
}
