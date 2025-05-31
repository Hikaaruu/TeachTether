using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanViewClassGroupStudentsHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<CanViewClassGroupStudentsRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanViewClassGroupStudentsRequirement requirement, int classGroupId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(classGroupId);
        if (classGroup == null)
            return;

        var school = await _unitOfWork.Schools.GetByIdAsync(classGroup.SchoolId);
        if (school == null)
            return;

        bool canView;
        switch (type)
        {
            case UserType.SchoolOwner:
                canView = school.SchoolOwnerId == (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id;
                break;
            case UserType.SchoolAdmin:
                canView = (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == school.Id;
                break;
            case UserType.Teacher:
            {
                var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                if (teacher == null)
                    return;

                var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                        .GetByClassGroupIdAsync(classGroupId))
                    .Select(cgs => cgs.Id);

                var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                    .AnyAsync(ca =>
                        ca.TeacherId == teacher.Id && classGroupSubjectIds.Contains(ca.ClassGroupSubjectId));

                var isHomeTeacher = classGroup.HomeroomTeacherId == teacher.Id;

                canView = isAssignedToClassGroup || isHomeTeacher;
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