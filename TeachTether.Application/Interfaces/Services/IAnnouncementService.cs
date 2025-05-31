using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services;

public interface IAnnouncementService
{
    Task<AnnouncementResponse> GetByIdAsync(int id);
    Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request, int teacherId);
    Task UpdateAsync(int id, UpdateAnnouncementRequest request);
    Task<IEnumerable<AnnouncementResponse>> GetAllForUserAsync(string userId);
    Task<IEnumerable<AnnouncementResponse>> GetAllBySchoolId(int schoolId);
    Task DeleteAsync(int id);
}