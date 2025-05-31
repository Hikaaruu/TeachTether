using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanManageAnnouncementHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<CanManageAnnouncementRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanManageAnnouncementRequirement requirement, int announcementId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);
        if (announcement is null)
            return;

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(announcement.TeacherId);
        if (teacher == null)
            return;

        var school = await _unitOfWork.Schools.GetByIdAsync(teacher.SchoolId);

        if (school == null)
            return;

        var canManage = false;

        switch (type)
        {
            case UserType.SchoolOwner:
                canManage = (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id == school.SchoolOwnerId;
                break;
            case UserType.SchoolAdmin:
                canManage = (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == school.Id;
                break;
            case UserType.Teacher:
            {
                var currentTeacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                if (currentTeacher == null)
                    return;

                canManage = announcement.TeacherId == currentTeacher.Id;
                break;
            }
            default:
                canManage = false;
                break;
        }

        if (canManage)
            context.Succeed(requirement);
    }
}