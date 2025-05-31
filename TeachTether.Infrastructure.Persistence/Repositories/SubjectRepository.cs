using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class SubjectRepository(ApplicationDbContext context) : Repository<Subject>(context), ISubjectRepository
{
    public async Task<Subject?> GetByNameAsync(string name)
    {
        return await _dbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name);
    }

    public async Task<IEnumerable<Subject>> GetBySchoolIdAsync(int schoolId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }
}