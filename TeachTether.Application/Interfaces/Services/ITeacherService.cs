using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface ITeacherService
    {
        Task<TeacherResponse> GetByIdAsync(int id);
        Task<IEnumerable<TeacherResponse>> GetAllBySchoolAsync(int schoolId);
        Task<CreatedTeacherResponse> CreateAsync(CreateTeacherRequest request, int schoolId);
        Task UpdateAsync(int id, UpdateTeacherRequest request);
        Task DeleteAsync(int id);
    }
}
