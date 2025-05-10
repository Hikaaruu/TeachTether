using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanModifyStudentRecordsHandler : AuthorizationHandler<CanModifyStudentRecordsRequirement, (int, int)>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanModifyStudentRecordsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanModifyStudentRecordsRequirement requirement, (int, int) resource)
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


                        canView = await _unitOfWork.ClassAssignments
                            .AnyAsync(ca => ca.TeacherId == teacher.Id &&
                            ca.ClassGroupSubjectId == classGroupSubjectId);

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
