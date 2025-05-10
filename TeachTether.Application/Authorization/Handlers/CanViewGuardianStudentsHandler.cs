using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewGuardianStudentsHandler : AuthorizationHandler<CanViewGuardianStudentsRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewGuardianStudentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewGuardianStudentsRequirement requirement, int guardianId)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            var type = Enum.Parse<UserType>(userType);

            var guardian = await _unitOfWork.Guardians.GetByIdAsync(guardianId);

            if (guardian == null)
                return;

            var school = await _unitOfWork.Schools.GetByIdAsync(guardian.SchoolId);

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
                        canView = guardian.UserId == userId;
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
