using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Announcement>> GetByAudienceAsync(AudienceType audience)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.TargetAudience == audience)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetByTeacherIdAsync(int teacherId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.TeacherId == teacherId)
                .ToListAsync();
        }
    }
}
