using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class ClassAssignmentRepository : Repository<ClassAssignment>, IClassAssignmentRepository
    {
        public ClassAssignmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClassAssignment>> GetByClassGroupIdAsync(int classGroupId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ca => ca.ClassGroupId == classGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassAssignment>> GetByTeacherIdAsync(int teacherId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ca => ca.TeacherId == teacherId)
                .ToListAsync();
        }
    }
}
