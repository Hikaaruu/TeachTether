using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassGroupStudentRepository : IRepository<ClassGroupStudent>
    {
        Task<IEnumerable<ClassGroupStudent>> GetByClassGroupIdAsync( int classGroupId);
        Task<ClassGroupStudent?> GetByStudentIdAsync(int studentId);
    }
}
