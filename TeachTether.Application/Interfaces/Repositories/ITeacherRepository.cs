using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<IEnumerable<Teacher>> GetBySchoolIdAsync(int schoolId);
    Task<Teacher?> GetByUserIdAsync(string userId);
}