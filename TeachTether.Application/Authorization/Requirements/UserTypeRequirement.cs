using Microsoft.AspNetCore.Authorization;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Requirements
{
    public class UserTypeRequirement : IAuthorizationRequirement
    {
        public UserType RequiredUserType { get; }

        public UserTypeRequirement(UserType requiredUserType)
        {
            RequiredUserType = requiredUserType;
        }
    }

}
