using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IStudentService
    {
        Task<StudentResponse> GetByIdAsync(int id);
        Task<IEnumerable<StudentResponse>> GetAllByClassGroupAsync(int classGroupId);
        Task<IEnumerable<StudentResponse>> GetAllBySchoolAsync(int schoolId);
        Task<CreatedStudentResponse> CreateAsync(CreateStudentRequest request, int schoolId);
        Task UpdateAsync(int id, UpdateStudentRequest request);
        Task DeleteAsync(int id);
    }
}
