using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewClassAssignmentsHandler : AuthorizationHandler<CanViewClassAssignmentsRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewClassAssignmentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewClassAssignmentsRequirement requirement, int classGroupId)
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

                        canView = classGroup.HomeroomTeacherId == teacher.Id;
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

                        canView = await _unitOfWork.ClassGroupStudents
                            .AnyAsync(cgs => cgs.ClassGroupId == classGroupId && studentIds.Contains(cgs.StudentId));

                        break;
                    }

                case UserType.Student:
                    {
                        var student = await _unitOfWork.Students.GetByUserIdAsync(userId);

                        if (student == null)
                            return;

                        canView = await _unitOfWork.ClassGroupStudents
                            .AnyAsync(cgs => cgs.ClassGroupId == classGroupId && cgs.StudentId == student.Id);

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
