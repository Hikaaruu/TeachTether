using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IClassGroupSubjectRepository : IRepository<ClassGroupSubject>
    {
        Task<IEnumerable<ClassGroupSubject>> GetBySubjectIdAsync(int subjectId);
        Task<IEnumerable<ClassGroupSubject>> GetByClassGroupIdAsync(int classGroupId);
    }
}
