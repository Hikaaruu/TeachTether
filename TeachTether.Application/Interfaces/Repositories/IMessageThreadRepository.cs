using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IMessageThreadRepository : IRepository<MessageThread>
{
    Task<IEnumerable<MessageThread>> GetByTeacherIdAsync(int teacherId);
    Task<IEnumerable<MessageThread>> GetByGuardianIdAsync(int guardianId);
}