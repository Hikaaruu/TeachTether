using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ISchoolRepository : IRepository<School>
    {
        Task<IEnumerable<School>> GetBySchoolOwnerIdAsync(int schoolOwnerId);
    }
}
