using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class TeacherRepository : Repository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Teacher>> GetBySchoolIdAsync(int schoolId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.SchoolId == schoolId)
                .ToListAsync();
        }

        public async Task<Teacher?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.UserId == userId);
        }
    }
}
