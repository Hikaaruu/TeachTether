using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanViewSubjectHandler(IUnitOfWork unitOfWork) : AuthorizationHandler<CanViewSubjectRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanViewSubjectRequirement requirement, int subjectId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId);

        if (subject == null)
            return;

        var school = await _unitOfWork.Schools.GetByIdAsync(subject.SchoolId);

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

                var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                        .GetBySubjectIdAsync(subjectId))
                    .Select(cgs => cgs.Id);

                var isTeachingSubject = await _unitOfWork.ClassAssignments
                    .AnyAsync(ca =>
                        ca.TeacherId == teacher.Id && classGroupSubjectIds.Contains(ca.ClassGroupSubjectId));

                var classGroupIds = (await _unitOfWork.ClassGroups.GetByHomeroomTeacherIdAsync(teacher.Id))
                    .Select(cg => cg.Id);

                var HRTeacher = await _unitOfWork.ClassGroupsSubjects
                    .AnyAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId) && cgs.SubjectId == subjectId);

                canView = isTeachingSubject || HRTeacher;
                break;
            }

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

                canView = await _unitOfWork.ClassGroupsSubjects
                    .AnyAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId) && cgs.SubjectId == subjectId);

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

                canView = await _unitOfWork.ClassGroupsSubjects
                    .AnyAsync(cgs => cgs.ClassGroupId == classGroupStudent.ClassGroupId && cgs.SubjectId == subjectId);

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