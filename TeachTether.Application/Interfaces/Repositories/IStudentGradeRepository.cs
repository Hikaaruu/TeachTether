using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IStudentGradeRepository : IRepository<StudentGrade>
{
    Task<IEnumerable<StudentGrade>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<StudentGrade>> GetByStudentAndSubjectIdAsync(int studentId, int subjectId);
}