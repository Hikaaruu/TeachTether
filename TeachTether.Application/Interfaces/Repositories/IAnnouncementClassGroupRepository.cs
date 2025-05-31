using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IAnnouncementClassGroupRepository : IRepository<AnnouncementClassGroup>
{
    Task<IEnumerable<AnnouncementClassGroup>> GetByClassGroupIdAsync(int classGroupId);
    Task<IEnumerable<AnnouncementClassGroup>> GetByAnnouncementIdAsync(int announcementId);
}