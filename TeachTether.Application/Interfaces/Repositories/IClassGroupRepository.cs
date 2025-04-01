using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassGroupRepository : IRepository<ClassGroup>
    {
        Task<IEnumerable<ClassGroup>> GetAllAsync(int schoolId);
    }
}
