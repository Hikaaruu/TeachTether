using Microsoft.EntityFrameworkCore;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class ClassGroupSubjectRepository : Repository<ClassGroupSubject>, IClassGroupSubjectRepository
    {
        public ClassGroupSubjectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClassGroupSubject>> GetByClassGroupIdAsync(int classGroupId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cgs => cgs.ClassGroupId == classGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassGroupSubject>> GetBySubjectIdAsync(int subjectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cgs => cgs.SubjectId == subjectId)
                .ToListAsync();
        }
    }
}
