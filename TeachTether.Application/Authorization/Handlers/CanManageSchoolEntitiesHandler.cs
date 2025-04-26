using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanManageSchoolEntitiesHandler : AuthorizationHandler<CanManageSchoolEntitiesRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanManageSchoolEntitiesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanManageSchoolEntitiesRequirement requirement, int schoolId)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            if (userType == UserType.SchoolOwner.ToString())
            {
                var schoolOwner = await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId);
                var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

                if (school is not null && schoolOwner is not null && school.SchoolOwnerId == schoolOwner.Id)
                {
                    context.Succeed(requirement);
                }
            }
            else if (userType == UserType.SchoolAdmin.ToString())
            {
                var schoolAdmin = await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId);

                if (schoolAdmin is not null && schoolAdmin.SchoolId == schoolId)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
