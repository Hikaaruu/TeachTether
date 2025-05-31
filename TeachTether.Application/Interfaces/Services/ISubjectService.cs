using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface ISubjectService
{
    Task<SubjectResponse> GetByIdAsync(int id);
    Task<IEnumerable<SubjectResponse>> GetAllBySchoolAsync(int schoolId);
    Task<IEnumerable<SubjectResponse>> GetAllByClassGroupAsync(int classGroupId);
    Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, int schoolId);
    Task UpdateAsync(int id, UpdateSubjectRequest request);
    Task DeleteAsync(int id);
}