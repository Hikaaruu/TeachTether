using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class SchoolOwnerRepository : Repository<SchoolOwner>, ISchoolOwnerRepository
    {
        public SchoolOwnerRepository(ApplicationDbContext context) : base(context) { }
    }
}
