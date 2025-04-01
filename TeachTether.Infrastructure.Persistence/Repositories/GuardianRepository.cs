using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class GuardianRepository : Repository<Guardian>, IGuardianRepository
    {
        public GuardianRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Guardian>> GetBySchoolIdAsync(int schoolId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(g => g.SchoolId == schoolId)
                .ToListAsync();
        }

        public async Task<Guardian?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(g => g.UserId == userId);
        }
    }
}
