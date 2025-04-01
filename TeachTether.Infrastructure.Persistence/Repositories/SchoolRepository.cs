using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class SchoolRepository : Repository<School>, ISchoolRepository
    {
        public SchoolRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<School>> GetBySchoolOwnerIdAsync(int schoolOwnerId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.SchoolOwnerId == schoolOwnerId)
                .ToListAsync();
        }
    }
}
