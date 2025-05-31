using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface ISchoolAdminRepository : IRepository<SchoolAdmin>
{
    Task<IEnumerable<SchoolAdmin>> GetBySchoolIdAsync(int schoolId);
    Task<SchoolAdmin?> GetByUserIdAsync(string userId);
}