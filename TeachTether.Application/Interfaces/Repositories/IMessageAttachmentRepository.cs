using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IMessageAttachmentRepository : IRepository<MessageAttachment>
{
    Task<IEnumerable<MessageAttachment>> GetByMessageIdAsync(int messageId);
    Task<MessageAttachment?> GetByFileUrlAsync(string fileUrl);
}