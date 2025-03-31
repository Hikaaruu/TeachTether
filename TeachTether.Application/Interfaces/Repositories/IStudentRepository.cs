using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task<IEnumerable<Student>> GetAllAsync(int schoolId);
    }
}
