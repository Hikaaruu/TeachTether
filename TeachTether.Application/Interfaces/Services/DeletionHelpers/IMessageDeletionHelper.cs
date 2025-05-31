namespace TeachTether.Application.Interfaces.Services.DeletionHelpers;

public interface IMessageDeletionHelper
{
    Task DeleteMessageAsync(int id);
}