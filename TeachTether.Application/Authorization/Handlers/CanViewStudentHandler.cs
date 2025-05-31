using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanViewStudentHandler(IUnitOfWork unitOfWork) : AuthorizationHandler<CanViewStudentRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanViewStudentRequirement requirement, int studentId)
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

                var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(student.Id);
                if (classGroupStudent == null)
                    return;

                var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(classGroupStudent.ClassGroupId);
                if (classGroup == null)
                    return;

                var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                        .GetByClassGroupIdAsync(classGroup.Id))
                    .Select(cgs => cgs.Id);

                var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                    .AnyAsync(ca =>
                        ca.TeacherId == teacher.Id && classGroupSubjectIds.Contains(ca.ClassGroupSubjectId));

                var isHomeTeacher = classGroup.HomeroomTeacherId == teacher.Id;

                canView = isAssignedToClassGroup || isHomeTeacher;
                break;
            }

            case UserType.Guardian:
            {
                var guardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId);

                if (guardian == null)
                    return;

                canView = await _unitOfWork.GuardianStudents
                    .AnyAsync(ga => ga.GuardianId == guardian.Id && ga.StudentId == student.Id);
                break;
            }

            case UserType.Student:
            {
                canView = (await _unitOfWork.Students.GetByUserIdAsync(userId))?.Id == student.Id;
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