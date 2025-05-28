using AutoMapper;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.Application.Services
{
    public class OwnerService : IOwnerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OwnerService(IUnitOfWork unitOfWork)
        { 
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.SchoolOwners.AnyAsync(so => so.Id == id);
        }
    }
}
