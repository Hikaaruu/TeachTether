using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IClassGroupService
    {
        Task<ClassGroupResponse> GetByIdAsync(int id);
        Task<IEnumerable<ClassGroupResponse>> GetAllBySchoolAsync(int schoolId);
        Task<ClassGroupResponse> CreateAsync(CreateClassGroupRequest request, int schoolId);
        Task UpdateAsync(int id, UpdateClassGroupRequest request);
        Task DeleteAsync(int id);
    }
}
