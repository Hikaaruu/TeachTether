using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers;

public class CanCreateAnnouncementHandler(IUnitOfWork unitOfWork)
    : AuthorizationHandler<CanCreateAnnouncementRequirement, List<int>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanCreateAnnouncementRequirement requirement, List<int> classGroupIds)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            return;

        var type = Enum.Parse<UserType>(userType);

        var classGroups = await _unitOfWork.ClassGroups.GetByIdsAsync(classGroupIds);

        if (type != UserType.Teacher)
            return;

        var teacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
        if (teacher == null)
            return;


        foreach (var cgId in classGroupIds)
        {
            var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                    .GetByClassGroupIdAsync(cgId))
                .Select(cgs => cgs.Id);

            var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                .AnyAsync(ca => ca.TeacherId == teacher.Id && classGroupSubjectIds.Contains(ca.ClassGroupSubjectId));

            var isHomeTeacher = classGroups.SingleOrDefault(cg => cg.Id == cgId)!.HomeroomTeacherId == teacher.Id;

            if (!(isAssignedToClassGroup || isHomeTeacher)) return;
        }

        context.Succeed(requirement);
    }
}