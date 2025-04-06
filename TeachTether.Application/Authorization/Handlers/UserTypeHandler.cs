using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class UserTypeHandler : AuthorizationHandler<UserTypeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UserTypeRequirement requirement)
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);

            if (roleClaim is not null &&
                Enum.TryParse<UserType>(roleClaim.Value, out var userType) &&
                userType == requirement.RequiredUserType)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}
