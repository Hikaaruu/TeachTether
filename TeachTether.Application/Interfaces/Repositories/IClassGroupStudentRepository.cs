using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassGroupStudentRepository
    {
        Task AddAsync(ClassGroupStudent classGroupStudent);
        Task UpdateAsync(ClassGroupStudent classGroupStudent);
        Task<IEnumerable<ClassGroupStudent>> GetAllAsync(int schoolId);
    }
}
