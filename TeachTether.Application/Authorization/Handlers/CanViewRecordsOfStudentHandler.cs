using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewRecordsOfStudentHandler : AuthorizationHandler<CanViewRecordsOfStudentRequirement, (int,int)>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewRecordsOfStudentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewRecordsOfStudentRequirement requirement, (int,int) resource)
        {
            var (studentId, subjectId) = resource;
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

            bool canView = false;

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
                            .GetAllAsync(cgs => cgs.SubjectId == subjectId && cgs.ClassGroupId == classGroup.Id))
                            .Select(cgs => cgs.Id);
                        var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                            .AnyAsync(ca => classGroupSubjectIds.Contains(ca.ClassGroupSubjectId) && ca.TeacherId == teacher.Id);

                        canView = classGroup.HomeroomTeacherId == teacher.Id || isAssignedToClassGroup;
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

                        canView = studentIds.Contains(studentId);
                        break;
                    }

                case UserType.Student:
                    {
                        canView = student.UserId == userId;
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
}
