using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanCreateMessageHandler : AuthorizationHandler<CanCreateMessageRequirement, MessageThreadResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanCreateMessageHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanCreateMessageRequirement requirement, MessageThreadResponse thread)
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

            bool canCreate;

            switch (type)
            {               
                case UserType.Guardian:
                    {
                        var currentGuardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId);

                        if (currentGuardian == null)
                            return;

                        canCreate = currentGuardian.Id == thread.GuardianId;

                        break;
                    }
                case UserType.Teacher:
                    {
                        var currentTeacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                        if (currentTeacher == null)
                            return;

                        canCreate = thread.TeacherId == currentTeacher.Id;

                        break;
                    }


                default:
                    canCreate = false;
                    break;
            }

            if (canCreate)
                context.Succeed(requirement);
        }
    }
}
