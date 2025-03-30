using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories
{
    public interface ISchoolOwnerRepository
    {
        Task AddAsync(SchoolOwner schoolOwner);
    }
}
