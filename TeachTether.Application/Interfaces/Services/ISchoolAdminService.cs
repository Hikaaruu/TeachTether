using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface ISchoolAdminService
    {
        Task<SchoolAdminResponse> GetByIdAsync(int id);
        Task<IEnumerable<SchoolAdminResponse>> GetAllBySchoolAsync(int schoolId);
        Task<CreatedSchoolAdminResponse> CreateAsync(CreateSchoolAdminRequest request, int schoolId);
        Task UpdateAsync(int id, UpdateSchoolAdminRequest request);
        Task DeleteAsync(int id);
    }
}
