using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IGuardianService
{
    Task<GuardianResponse> GetByIdAsync(int id);
    Task<IEnumerable<GuardianResponse>> GetAllBySchoolAsync(int schoolId);
    Task<IEnumerable<GuardianResponse>> GetAvailableForTeacherAsync(int teacherId);
    Task<IEnumerable<GuardianResponse>> GetAllByStudentAsync(int studentId);
    Task<CreatedGuardianResponse> CreateAsync(CreateGuardianRequest request, int schoolId);
    Task UpdateAsync(int id, UpdateGuardianRequest request);
    Task DeleteAsync(int id);
}