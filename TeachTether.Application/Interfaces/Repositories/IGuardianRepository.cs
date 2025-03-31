using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IGuardianRepository
    {
        Task AddAsync(Guardian guardian);
        Task UpdateAsync(Guardian guardian);
        Task<IEnumerable<Guardian>> GetAllAsync(int schoolId);
    }
}
