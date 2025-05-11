using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanViewThreadHandler : AuthorizationHandler<CanViewThreadRequirement, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanViewThreadHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanViewThreadRequirement requirement, int threadId)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            var role = Enum.Parse<UserType>(userType);

            var thread = await _unitOfWork.MessageThreads.GetByIdAsync(threadId);
            if (thread == null)
                return;

            var schoolId = (await _unitOfWork.Teachers.GetByIdAsync(thread.TeacherId))?.SchoolId;
            if (schoolId == null) 
                return;

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId.Value);
            if (school == null)
                return;

            var canView = role switch
            {
                UserType.SchoolOwner => school.SchoolOwnerId == (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id,
                UserType.SchoolAdmin => (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == schoolId,
                UserType.Teacher => (await _unitOfWork.Teachers.GetByUserIdAsync(userId))?.Id == thread.TeacherId,
                UserType.Guardian => (await _unitOfWork.Guardians.GetByUserIdAsync(userId))?.Id == thread.GuardianId,

                _ => false
            };

            if (canView)
                context.Succeed(requirement);
        }
    }
}
