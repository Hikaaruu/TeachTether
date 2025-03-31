using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassGroupRepository
    {
        Task AddAsync(ClassGroup classGroup);
        Task UpdateAsync(ClassGroup classGroup);
        Task<IEnumerable<ClassGroup>> GetAllAsync(int schoolId);
    }
}
