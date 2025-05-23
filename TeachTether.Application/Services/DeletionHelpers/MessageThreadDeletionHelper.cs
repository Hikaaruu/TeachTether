using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers
{
    public class MessageThreadDeletionHelper : IMessageThreadDeletionHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageDeletionHelper _messageDeletionHelper;

        public MessageThreadDeletionHelper(IUnitOfWork unitOfWork, IMessageDeletionHelper messageDeletionHelper)
        { 
            _unitOfWork = unitOfWork;
            _messageDeletionHelper = messageDeletionHelper;
        }

        public async Task DeleteMessageThreadAsync(int id)
        {
            var thread = await _unitOfWork.MessageThreads.GetByIdAsync(id)
                ?? throw new NotFoundException("Thread not found");
            var messages = await _unitOfWork.Messages.GetByThreadIdAsync(id);
            foreach (var message in messages)
            {
                await _messageDeletionHelper.DeleteMessageAsync(message.Id);
            }
            _unitOfWork.MessageThreads.Delete(thread);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
