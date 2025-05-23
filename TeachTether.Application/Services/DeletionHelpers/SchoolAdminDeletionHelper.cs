using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class SchoolAdminDeletionHelper : ISchoolAdminDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public SchoolAdminDeletionHelper(IUnitOfWork unitOfWork, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task DeleteSchoolAdminAsync(int id)
        {
            var admin = await _unitOfWork.SchoolAdmins.GetByIdAsync(id)
                ?? throw new NotFoundException("Admin not found");

            _unitOfWork.SchoolAdmins.Delete(admin);

            var result = await _userRepository.DeleteAsync(admin.UserId);

            if (result.Succeeded)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
