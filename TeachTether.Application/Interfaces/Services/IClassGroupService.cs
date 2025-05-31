using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IClassGroupService
{
    Task<ClassGroupResponse> GetByIdAsync(int id);
    Task<ClassGroupResponse> GetByStudentAsync(int studentId);
    Task<IEnumerable<ClassGroupResponse>> GetAllBySchoolAsync(int schoolId);
    Task<IEnumerable<ClassGroupResponse>> GetAllByTeacherAsync(int teacherId);
    Task<IEnumerable<ClassGroupResponse>> GetAvailableForTeacherAsync(int teacherId);
    Task<ClassGroupResponse> CreateAsync(CreateClassGroupRequest request, int schoolId);
    Task UpdateAsync(int id, UpdateClassGroupRequest request);
    Task DeleteAsync(int id);
}