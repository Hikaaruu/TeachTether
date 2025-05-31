using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<IEnumerable<Student>> GetBySchoolIdAsync(int schoolId);
    Task<Student?> GetByUserIdAsync(string userId);
}