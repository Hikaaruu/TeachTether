using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ISchoolRepository
    {
        Task AddAsync(School school);
        Task UpdateAsync(School school);
        Task<IEnumerable<School>> GetAllAsync(int schoolOwnerId);
    }
}
