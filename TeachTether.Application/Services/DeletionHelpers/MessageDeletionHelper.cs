using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class MessageDeletionHelper : IMessageDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageDeletionHelper(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task DeleteMessageAsync(int id)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id)
                ?? throw new NotFoundException("Message not found");
            var messageAtts = await _unitOfWork.MessageAttachments.GetByMessageIdAsync(id);
            _unitOfWork.MessageAttachments.DeleteMany(messageAtts);
            _unitOfWork.Messages.Delete(message);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
