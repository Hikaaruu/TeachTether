using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ISchoolOwnerRepository : IRepository<SchoolOwner>
    {
        Task<SchoolOwner?> GetByUserIdAsync(string userId);
    }
}
