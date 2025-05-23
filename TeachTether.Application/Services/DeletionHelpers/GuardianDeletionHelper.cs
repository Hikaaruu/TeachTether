using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class GuardianDeletionHelper : IGuardianDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IMessageThreadDeletionHelper _messageThreadDeletionHelper;

        public GuardianDeletionHelper(IUnitOfWork unitOfWork, IUserRepository userRepository, IMessageThreadDeletionHelper messageThreadDeletionHelper)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _messageThreadDeletionHelper = messageThreadDeletionHelper;
        }
        public async Task DeleteGuardianAsync(int id)
        {
            var guardian = await _unitOfWork.Guardians.GetByIdAsync(id)
                ?? throw new NotFoundException("Guardian not found");

            var gs = await _unitOfWork.GuardianStudents.GetByGuardianIdAsync(id);
            _unitOfWork.GuardianStudents.DeleteMany(gs);

            var threads = await _unitOfWork.MessageThreads.GetByGuardianIdAsync(id);

            foreach (var t in threads)
            {
                await _messageThreadDeletionHelper.DeleteMessageThreadAsync(t.Id);
            }

            _unitOfWork.Guardians.Delete(guardian);
            var result = await _userRepository.DeleteAsync(guardian.UserId);

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
