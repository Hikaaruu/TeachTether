using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassAssignmentRepository : IRepository<ClassAssignment>
    {
        Task<IEnumerable<ClassAssignment>> GetAllAsync(int schoolId);
    }
}
