using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class AnnouncementClassGroupRepository : Repository<AnnouncementClassGroup>, IAnnouncementClassGroupRepository
    {
        public AnnouncementClassGroupRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<AnnouncementClassGroup>> GetByAnnouncementIdAsync(int announcementId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(acg => acg.AnnouncementId == announcementId)
                .ToListAsync();
        }

        public  async Task<IEnumerable<AnnouncementClassGroup>> GetByClassGroupIdAsync(int classGroupId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(acg => acg.ClassGroupId == classGroupId)
                .ToListAsync();
        }
    }
}
