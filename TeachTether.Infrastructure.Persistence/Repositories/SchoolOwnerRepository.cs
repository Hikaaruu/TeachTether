
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class SchoolOwnerRepository : ISchoolOwnerRepository
    {
        private readonly ApplicationDbContext _context;

        public SchoolOwnerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SchoolOwner schoolOwner)
        {
            await _context.SchoolOwners.AddAsync(schoolOwner);
            await _context.SaveChangesAsync();
        }
    }
}
