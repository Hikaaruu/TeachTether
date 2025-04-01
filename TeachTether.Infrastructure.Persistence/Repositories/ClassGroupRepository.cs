using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class ClassGroupRepository : Repository<ClassGroup>, IClassGroupRepository
    {
        public ClassGroupRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClassGroup>> GetByHomeroomTeacherIdAsync(int homeroomTeacherId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cg => cg.HomeroomTeacherId == homeroomTeacherId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassGroup>> GetBySchoolIdAsync(int schoolId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cg => cg.SchoolId == schoolId)
                .ToListAsync();
        }
    }
}
