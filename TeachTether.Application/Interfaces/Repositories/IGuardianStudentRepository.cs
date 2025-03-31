using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IGuardianStudentRepository
    {
        Task AddAsync(GuardianStudent guardianStudent);
        Task UpdateAsync(GuardianStudent guardianStudent);
        Task<IEnumerable<GuardianStudent>> GetAllAsync(int schoolId);
    }
}
