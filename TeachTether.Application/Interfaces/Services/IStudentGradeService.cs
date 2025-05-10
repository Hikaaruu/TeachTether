using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IStudentGradeService
    {
        Task<StudentGradeResponse> GetByIdAsync(int id);
        Task<IEnumerable<StudentGradeResponse>> GetAllByStudentAsync(int studentId);
        Task<StudentGradeResponse> CreateAsync(CreateStudentGradeRequest request, int teacherId, int studentId);
        Task UpdateAsync(int id, UpdateStudentGradeRequest request);
        Task DeleteAsync(int id);
    }
}
