using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IClassAssignmentService
    {
        Task CreateAsync(CreateClassAssignmentRequest request, int classGroupId, int subjectId);
        Task DeleteAsync(int classGroupId, int subjectId, int teacherId);
    }
}
