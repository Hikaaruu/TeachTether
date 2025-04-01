using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class GuardianStudentRepository : Repository<GuardianStudent>, IGuardianStudentRepository
    {
        public GuardianStudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<GuardianStudent>> GetByGuardianIdAsync(int guardianId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(gs => gs.GuardianId == guardianId)
                .ToListAsync();
        }

        public async Task<IEnumerable<GuardianStudent>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(gs => gs.StudentId == studentId)
                .ToListAsync();
        }
    }
}
