using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;
namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class MessageAttachmentRepository : Repository<MessageAttachment>, IMessageAttachmentRepository
    {
        public MessageAttachmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<MessageAttachment?> GetByFileUrlAsync(string fileUrl)
        {
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(ma => ma.FileUrl == fileUrl);
        }

        public async Task<IEnumerable<MessageAttachment>> GetByMessageIdAsync(int messageId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ma => ma.MessageId == messageId)
                .ToListAsync();
        }
    }
}
