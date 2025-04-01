using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IGuardianRepository : IRepository<Guardian>
    {
        Task<IEnumerable<Guardian>> GetAllAsync(int schoolId);
    }
}
