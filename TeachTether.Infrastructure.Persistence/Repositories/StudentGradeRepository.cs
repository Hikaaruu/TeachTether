using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories;

public class StudentGradeRepository(ApplicationDbContext context)
    : Repository<StudentGrade>(context), IStudentGradeRepository
{
    public async Task<IEnumerable<StudentGrade>> GetByStudentAndSubjectIdAsync(int studentId, int subjectId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(sg => sg.StudentId == studentId && sg.SubjectId == subjectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentGrade>> GetByStudentIdAsync(int studentId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(sg => sg.StudentId == studentId)
            .ToListAsync();
    }
}