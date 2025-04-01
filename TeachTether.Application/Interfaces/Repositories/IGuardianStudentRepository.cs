using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IGuardianStudentRepository : IRepository<GuardianStudent>
    {
        Task<IEnumerable<GuardianStudent>> GetByGuardianIdAsync(int guardianId);
        Task<IEnumerable<GuardianStudent>> GetByStudentIdAsync(int studentId);
    }
}
