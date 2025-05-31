using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class SchoolOwnerRepository(ApplicationDbContext context)
    : Repository<SchoolOwner>(context), ISchoolOwnerRepository
{
    public async Task<SchoolOwner?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(so => so.UserId == userId);
    }
}