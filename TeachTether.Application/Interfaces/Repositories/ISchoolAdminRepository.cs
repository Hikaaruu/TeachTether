using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ISchoolAdminRepository
    {
        Task AddAsync(SchoolAdmin schoolAdmin);
        Task UpdateAsync(SchoolAdmin schoolAdmin);
        Task<IEnumerable<SchoolAdmin>> GetAllAsync(int schoolId);

    }
}
