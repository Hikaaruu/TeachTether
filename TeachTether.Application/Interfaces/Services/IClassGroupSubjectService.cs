using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IClassGroupSubjectService
{
    Task CreateAsync(CreateClassGroupSubjectRequest request, int classGroupId);
    Task DeleteAsync(int classGroupId, int subjectId);
}