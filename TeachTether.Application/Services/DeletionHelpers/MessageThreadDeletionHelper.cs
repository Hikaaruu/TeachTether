using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers;

public class MessageThreadDeletionHelper(IUnitOfWork unitOfWork, IMessageDeletionHelper messageDeletionHelper)
    : IMessageThreadDeletionHelper
{
    private readonly IMessageDeletionHelper _messageDeletionHelper = messageDeletionHelper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task DeleteMessageThreadAsync(int id)
    {
        var thread = await _unitOfWork.MessageThreads.GetByIdAsync(id)
                     ?? throw new NotFoundException("Thread not found");
        var messages = await _unitOfWork.Messages.GetByThreadIdAsync(id);
        foreach (var message in messages) await _messageDeletionHelper.DeleteMessageAsync(message.Id);
        _unitOfWork.MessageThreads.Delete(thread);
        await _unitOfWork.SaveChangesAsync();
    }
}