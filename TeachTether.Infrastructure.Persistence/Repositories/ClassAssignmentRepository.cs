using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class ClassAssignmentRepository : Repository<ClassAssignment>, IClassAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassAssignmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassAssignment>> GetAllAsync(int schoolId)
        {
            throw new NotImplementedException();
        }
    }
}
