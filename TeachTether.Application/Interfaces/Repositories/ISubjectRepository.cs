using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ISubjectRepository : IRepository<Subject>
    {
        Task<Subject?> GetByNameAsync(string name);
    }
}
