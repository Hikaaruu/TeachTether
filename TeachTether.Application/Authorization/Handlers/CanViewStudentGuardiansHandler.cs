using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanViewStudentGuardiansHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<CanViewStudentGuardiansRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanViewStudentGuardiansRequirement requirement, int studentId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        var student = await _unitOfWork.Students.GetByIdAsync(studentId);

        if (student == null)
            return;

        var school = await _unitOfWork.Schools.GetByIdAsync(student.SchoolId);

        if (school == null)
            return;

        bool canView;

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
                var guardian = _unitOfWork.Guardians.GetByUserIdAsync(userId);
                if (guardian == null)
                    return;

                var studentIds = (await _unitOfWork.GuardianStudents
                        .GetByGuardianIdAsync(guardian.Id))
                    .Select(gs => gs.StudentId);

                canView = studentIds.Contains(student.Id);
                break;
            }

            case UserType.Student:
            {
                canView = student.UserId == userId;
                break;
            }

            case UserType.Teacher:
            {
                var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                if (teacher == null)
                    return;

                var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(student.Id);

                if (classGroupStudent == null)
                    return;

                var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(classGroupStudent.ClassGroupId);

                if (classGroup == null)
                    return;

                canView = classGroup.HomeroomTeacherId == teacher.Id;
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