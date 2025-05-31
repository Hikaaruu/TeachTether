using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;

namespace TeachTether.Application.Services.DeletionHelpers;

public class AnnouncementDeletionHelper(IUnitOfWork unitOfWork) : IAnnouncementDeletionHelper
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task DeleteAnnouncementAsync(int id)
    {
        var ann = await _unitOfWork.Announcements.GetByIdAsync(id)
                  ?? throw new NotFoundException("Announcement not found");

        var annClassGroups = await _unitOfWork.AnnouncementClassGroups.GetByAnnouncementIdAsync(id);
        _unitOfWork.AnnouncementClassGroups.DeleteMany(annClassGroups);
        _unitOfWork.Announcements.Delete(ann);
        await _unitOfWork.SaveChangesAsync();
    }
}