using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanManageSchoolHandler(IUnitOfWork unitOfWork) : AuthorizationHandler<CanManageSchoolRequirement, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanManageSchoolRequirement requirement, int schoolId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        if (userType != UserType.SchoolOwner.ToString())
            return;

        var schoolOwner = await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId);
        var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

        if (school is null || schoolOwner is null)
            return;

        if (school.SchoolOwnerId == schoolOwner.Id) context.Succeed(requirement);
    }
}