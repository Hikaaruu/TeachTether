using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface ITeacherService
{
    Task<TeacherResponse> GetByIdAsync(int id);
    Task<IEnumerable<TeacherResponse>> GetAllBySchoolAsync(int schoolId);
    Task<IEnumerable<TeacherResponse>> GetAvailableForGuardianAsync(int guardianId);
    Task<IEnumerable<TeacherResponse>> GetAllByClassGroupSubjectAsync(int classGroupId, int subjectId);
    Task<CreatedTeacherResponse> CreateAsync(CreateTeacherRequest request, int schoolId);
    Task UpdateAsync(int id, UpdateTeacherRequest request);
    Task DeleteAsync(int id);
}