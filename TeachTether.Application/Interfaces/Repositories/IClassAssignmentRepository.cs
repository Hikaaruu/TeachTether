using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassAssignmentRepository
    {
        Task AddAsync(ClassAssignment classAssignment);
        Task UpdateAsync(ClassAssignment classAssignment);
        Task<IEnumerable<ClassAssignment>> GetAllAsync(int schoolId);
    }
}
