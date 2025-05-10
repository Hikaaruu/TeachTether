using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IStudentBehaviorService
    {
        Task<StudentBehaviorResponse> GetByIdAsync(int id);
        Task<IEnumerable<StudentBehaviorResponse>> GetAllByStudentAsync(int studentId);
        Task<StudentBehaviorResponse> CreateAsync(CreateStudentBehaviorRequest request, int teacherId, int studentId);
        Task UpdateAsync(int id, UpdateStudentBehaviorRequest request);
        Task DeleteAsync(int id);
    }
}
