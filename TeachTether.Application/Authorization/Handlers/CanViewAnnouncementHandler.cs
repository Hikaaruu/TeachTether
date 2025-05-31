using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanViewAnnouncementHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<CanViewAnnouncementRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanViewAnnouncementRequirement requirement, int announcementId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;
        var type = Enum.Parse<UserType>(userType);

        var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);
        var allowedClassGroupIds = (await _unitOfWork.AnnouncementClassGroups
                .GetByAnnouncementIdAsync(announcementId))
            .Select(acg => acg.ClassGroupId);

        if (announcement == null)
            return;

        var teacher = await _unitOfWork.Teachers.GetByIdAsync(announcement.TeacherId);

        if (teacher == null)
            return;

        var school = await _unitOfWork.Schools.GetByIdAsync(teacher.SchoolId);

        if (school == null)
            return;

        var canView = false;

        switch (type)
        {
            case UserType.SchoolOwner:
                canView = (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id == school.SchoolOwnerId;
                break;

            case UserType.SchoolAdmin:
                canView = (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == school.Id;
                break;

            case UserType.Guardian:
            {
                var guardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId);

                if (guardian == null)
                    return;


                var studentIds = (await _unitOfWork.GuardianStudents
                        .GetByGuardianIdAsync(guardian.Id))
                    .Select(gs => gs.StudentId);

                if (!studentIds.Any())
                    return;

                var classGroupIds = (await _unitOfWork.ClassGroupStudents
                        .GetAllAsync(cgs => studentIds.Contains(cgs.StudentId)))
                    .Select(cgs => cgs.ClassGroupId);

                if (!classGroupIds.Any())
                    return;

                canView = allowedClassGroupIds.Any(x => classGroupIds.Contains(x)) &&
                          (announcement.TargetAudience == AudienceType.Guardian ||
                           announcement.TargetAudience == AudienceType.StudentAndGuardian);

                break;
            }

            case UserType.Student:
            {
                var student = await _unitOfWork.Students.GetByUserIdAsync(userId);
                if (student == null)
                    return;

                var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(student.Id);
                if (classGroupStudent == null)
                    return;
                canView = allowedClassGroupIds.Contains(classGroupStudent.ClassGroupId) &&
                          (announcement.TargetAudience == AudienceType.Student ||
                           announcement.TargetAudience == AudienceType.StudentAndGuardian);
                break;
            }
            case UserType.Teacher:
            {
                var currentTeacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                if (currentTeacher == null)
                    return;

                canView = teacher.Id == currentTeacher.Id;

                break;
            }

            default:
                canView = false;
                break;
        }

        if (canView)
            context.Succeed(requirement);
    }
}