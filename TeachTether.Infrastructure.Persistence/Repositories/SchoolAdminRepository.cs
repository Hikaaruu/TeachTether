using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class SchoolAdminRepository(ApplicationDbContext context)
    : Repository<SchoolAdmin>(context), ISchoolAdminRepository
{
    public async Task<IEnumerable<SchoolAdmin>> GetBySchoolIdAsync(int schoolId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(sa => sa.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<SchoolAdmin?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(sa => sa.UserId == userId);
    }
}