using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;
namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class StudentAttendanceRepository : Repository<StudentAttendance>, IStudentAttendanceRepository
    {
        public StudentAttendanceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<StudentAttendance>> GetByStudentAndSubjectIdAsync(int studentId, int subjectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sa => sa.StudentId == studentId && sa.SubjectId == subjectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentAttendance>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sa => sa.StudentId == studentId)
                .ToListAsync();
        }
    }
}
