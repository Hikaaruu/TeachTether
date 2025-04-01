using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class ClassGroupStudentRepository : Repository<ClassGroupStudent>, IClassGroupStudentRepository
    {
        public ClassGroupStudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClassGroupStudent>> GetByClassGroupIdAsync(int classGroupId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cgs => cgs.ClassGroupId == classGroupId)
                .ToListAsync();
        }

        public async Task<ClassGroupStudent?> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(cgs => cgs.StudentId == studentId);
        }
    }
}
