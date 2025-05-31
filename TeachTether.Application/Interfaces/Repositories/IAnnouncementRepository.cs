using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<IEnumerable<Announcement>> GetByAudienceAsync(AudienceType audience);
    Task<IEnumerable<Announcement>> GetByTeacherIdAsync(int teacherId);
}