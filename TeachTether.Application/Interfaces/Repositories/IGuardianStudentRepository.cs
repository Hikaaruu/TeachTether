using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IGuardianStudentRepository : IRepository<GuardianStudent>
    {
        Task<IEnumerable<GuardianStudent>> GetAllAsync(int schoolId);
    }
}
