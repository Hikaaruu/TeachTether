using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IGuardianRepository : IRepository<Guardian>
    {
        Task<IEnumerable<Guardian>> GetBySchoolIdAsync(int schoolId);
        Task<Guardian?> GetByUserIdAsync(string userId);
    }
}
