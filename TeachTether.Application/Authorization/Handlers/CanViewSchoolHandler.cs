using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewSchoolHandler : AuthorizationHandler<CanViewSchoolRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewSchoolHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewSchoolRequirement requirement, int schoolId)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            var role = Enum.Parse<UserType>(userType);
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

            if (school == null)
                return;

            var canView = role switch
            {
                UserType.SchoolOwner => school.SchoolOwnerId ==
                (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id,

                UserType.SchoolAdmin => (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == schoolId,
                UserType.Teacher => (await _unitOfWork.Teachers.GetByUserIdAsync(userId))?.SchoolId == schoolId,
                UserType.Student => (await _unitOfWork.Students.GetByUserIdAsync(userId))?.SchoolId == schoolId,
                UserType.Guardian => (await _unitOfWork.Guardians.GetByUserIdAsync(userId))?.SchoolId == schoolId,

                _ => false
            };

            if (canView)
                context.Succeed(requirement);
        }
    }
}
