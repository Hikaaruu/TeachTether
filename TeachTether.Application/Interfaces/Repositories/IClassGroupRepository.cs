using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IClassGroupRepository : IRepository<ClassGroup>
{
    Task<IEnumerable<ClassGroup>> GetBySchoolIdAsync(int schoolId);
    Task<IEnumerable<ClassGroup>> GetByHomeroomTeacherIdAsync(int homeroomTeacherId);
}