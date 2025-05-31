using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanCreateStudentRecordsHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<CanCreateStudentRecordsRequirement, (int, int)>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanCreateStudentRecordsRequirement requirement, (int, int) resource)
    {
        var (studentId, subjectId) = resource;


        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        if (type != UserType.Teacher)
            return;

        var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
        if (teacher == null)
            return;

        var classGroupStudent = await _unitOfWork.ClassGroupStudents.GetByStudentIdAsync(studentId);

        if (classGroupStudent == null)
            return;

        var classGroup = await _unitOfWork.ClassGroups.GetByIdAsync(classGroupStudent.ClassGroupId);

        if (classGroup == null)
            return;

        var classGroupSubjectId = (await _unitOfWork.ClassGroupsSubjects
                .GetByClassGroupIdAsync(classGroup.Id))
            .Where(cgs => cgs.SubjectId == subjectId)
            .Select(cgs => cgs.Id).SingleOrDefault();

        if (await _unitOfWork.ClassAssignments
                .AnyAsync(ca => ca.TeacherId == teacher.Id &&
                                ca.ClassGroupSubjectId == classGroupSubjectId))
            context.Succeed(requirement);
    }
}