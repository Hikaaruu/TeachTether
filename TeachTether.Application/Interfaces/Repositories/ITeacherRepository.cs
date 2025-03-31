using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ITeacherRepository
    {
        Task AddAsync(Teacher teacher);
        Task UpdateAsync(Teacher teacher);
        Task<IEnumerable<Teacher>> GetAllAsync(int schoolId);
    }
}
