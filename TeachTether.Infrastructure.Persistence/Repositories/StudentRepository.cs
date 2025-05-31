using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class StudentRepository(ApplicationDbContext context) : Repository<Student>(context), IStudentRepository
{
    public async Task<IEnumerable<Student>> GetBySchoolIdAsync(int schoolId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<Student?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId);
    }
}