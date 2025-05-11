using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanDeleteThreadHandler : AuthorizationHandler<CanDeleteThreadRequirement, MessageThread>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanDeleteThreadHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanDeleteThreadRequirement requirement, MessageThread thread)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            var type = Enum.Parse<UserType>(userType);

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(thread.TeacherId);

            if (teacher == null)
                return;

            var guardian = await _unitOfWork.Guardians.GetByIdAsync(thread.GuardianId);

            if (guardian == null)
                return;

            var school = await _unitOfWork.Schools.GetByIdAsync(teacher.SchoolId);

            if (school == null)
                return;

            bool canDelete = false;

            switch (type)
            {
                case UserType.SchoolOwner:
                    canDelete = (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id == school.SchoolOwnerId;
                    break;

                case UserType.SchoolAdmin:
                    canDelete = (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == school.Id;
                    break;

                case UserType.Guardian:
                    {
                        var currentGuardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId);

                        if (currentGuardian == null)
                            return;

                        canDelete = currentGuardian.Id == thread.GuardianId;

                        break;
                    }
                case UserType.Teacher:
                    {
                        var currentTeacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                        if (currentTeacher == null)
                            return;

                        canDelete = thread.TeacherId == currentTeacher.Id;

                        break;
                    }


                default:
                    canDelete = false;
                    break;
            }

            if (canDelete)
                context.Succeed(requirement);
        }

    }
}
