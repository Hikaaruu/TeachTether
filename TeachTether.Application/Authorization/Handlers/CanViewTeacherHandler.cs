using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewTeacherHandler : AuthorizationHandler<CanViewTeacherRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewTeacherHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewTeacherRequirement requirement, int teacherId)
        {

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            var type = Enum.Parse<UserType>(userType);

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);

            if (teacher == null)
                return;

            var school = await _unitOfWork.Schools.GetByIdAsync(teacher.SchoolId);

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

                        var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                            .GetAllAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId)))
                            .Select(cgs => cgs.Id);

                        var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                            .AnyAsync(ca => classGroupSubjectIds.Contains(ca.ClassGroupSubjectId) && ca.TeacherId == teacherId);

                        var isHomeTeacher = await _unitOfWork.ClassGroups
                            .AnyAsync(cg => classGroupIds.Contains(cg.Id) && 
                            cg.HomeroomTeacherId == teacherId);

                        canView = isAssignedToClassGroup || isHomeTeacher;

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

                        var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                            .GetByClassGroupIdAsync(classGroupStudent.ClassGroupId))
                            .Select(cgs => cgs.Id);

                        var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                            .AnyAsync(ca => classGroupSubjectIds.Contains(ca.ClassGroupSubjectId) && ca.TeacherId == teacherId);

                        var isHomeTeacher = await _unitOfWork.ClassGroups
                            .AnyAsync(cg => cg.Id == classGroupStudent.ClassGroupId &&
                            cg.HomeroomTeacherId == teacherId);

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
}
