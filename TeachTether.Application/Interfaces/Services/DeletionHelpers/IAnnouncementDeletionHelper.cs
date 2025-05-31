namespace TeachTether.Application.Interfaces.Services.DeletionHelpers;

public interface IAnnouncementDeletionHelper
{
    Task DeleteAnnouncementAsync(int id);
}