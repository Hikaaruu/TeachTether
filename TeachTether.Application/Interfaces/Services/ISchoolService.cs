using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface ISchoolService
{
    Task<SchoolResponse> GetByIdAsync(int id);
    Task<IEnumerable<SchoolResponse>> GetAllByOwnerAsync(int schoolOwnerId);
    Task<SchoolResponse> CreateAsync(CreateSchoolRequest request, int schoolOwnerId);
    Task UpdateAsync(int id, UpdateSchoolRequest request);
    Task DeleteAsync(int id);
}