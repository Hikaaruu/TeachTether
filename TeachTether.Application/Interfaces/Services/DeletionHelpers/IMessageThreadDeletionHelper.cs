namespace TeachTether.Application.Interfaces.Services.DeletionHelpers
{
    public interface IMessageThreadDeletionHelper
    {
        Task DeleteMessageThreadAsync(int id);
    }
}
