using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewClassGroupStudentsHandler : AuthorizationHandler<CanViewClassGroupStudentsRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewClassGroupStudentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewClassGroupStudentsRequirement requirement, int classGroupId)
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
                        {
                            canView = false;
                            break;
                        }

                        var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                            .ExistsAsync(ca => ca.TeacherId == teacher.Id && ca.ClassGroupId == classGroup.Id);

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
}
