using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class StudentBehaviorRepository : Repository<StudentBehavior>, IStudentBehaviorRepository
    {
        public StudentBehaviorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<StudentBehavior>> GetByStudentAndSubjectIdAsync(int studentId, int subjectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sb => sb.StudentId == studentId && sb.SubjectId == subjectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentBehavior>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sb => sb.StudentId == studentId)
                .ToListAsync();
        }
    }
}
