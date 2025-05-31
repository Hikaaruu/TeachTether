using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.Application.Services;

public class OwnerService(IUnitOfWork unitOfWork) : IOwnerService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.SchoolOwners.AnyAsync(so => so.Id == id);
    }
}