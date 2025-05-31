using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanViewGuardianHandler(IUnitOfWork unitOfWork) : AuthorizationHandler<CanViewGuardianRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanViewGuardianRequirement requirement, int guardianId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        var guardian = await _unitOfWork.Guardians.GetByIdAsync(guardianId);

        if (guardian == null)
            return;

        var school = await _unitOfWork.Schools.GetByIdAsync(guardian.SchoolId);

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

            case UserType.Teacher:
            {
                var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                if (teacher == null)
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

                var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                        .GetAllAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId)))
                    .Select(cgs => cgs.Id);

                var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                    .AnyAsync(ca =>
                        classGroupSubjectIds.Contains(ca.ClassGroupSubjectId) && ca.TeacherId == teacher.Id);

                var isHomeTeacher = await _unitOfWork.ClassGroups
                    .AnyAsync(cg => classGroupIds.Contains(cg.Id) &&
                                    cg.HomeroomTeacherId == teacher.Id);

                canView = isAssignedToClassGroup || isHomeTeacher;

                break;
            }

            case UserType.Student:
            {
                var student = await _unitOfWork.Students.GetByUserIdAsync(userId);
                if (student == null)
                    return;

                canView = await _unitOfWork.GuardianStudents.AnyAsync(gs => gs.StudentId == student.Id
                                                                            && gs.GuardianId == guardianId);

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