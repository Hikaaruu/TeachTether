using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassGroupStudentRepository : IRepository<ClassGroupStudent>
    {
        Task<IEnumerable<ClassGroupStudent>> GetAllAsync(int schoolId);
    }
}
