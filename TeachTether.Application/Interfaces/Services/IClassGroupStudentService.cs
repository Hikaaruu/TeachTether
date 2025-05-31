using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IClassGroupStudentService
{
    Task CreateAsync(CreateClassGroupStudentRequest request, int classGroupId);
    Task DeleteAsync(int classGroupId, int studentId);
}