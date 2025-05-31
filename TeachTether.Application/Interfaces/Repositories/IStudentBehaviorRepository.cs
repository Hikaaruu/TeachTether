using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IStudentBehaviorRepository : IRepository<StudentBehavior>
{
    Task<IEnumerable<StudentBehavior>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<StudentBehavior>> GetByStudentAndSubjectIdAsync(int studentId, int subjectId);
}