using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassAssignmentRepository : IRepository<ClassAssignment>
    {
        Task<IEnumerable<ClassAssignment>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<ClassAssignment>> GetByClassGroupIdAsync(int classGroupId);
    }
}
