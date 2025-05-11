using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Authorization.Handlers
{
    public class CanCreateThreadHandler : AuthorizationHandler<CanCreateThreadRequirement, (int, int)>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CanCreateThreadHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanCreateThreadRequirement requirement, (int, int) resource)
        {
            var (teacherId, guardianId) = resource;

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userType = context.User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return;

            var type = Enum.Parse<UserType>(userType);

            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);

            if (teacher == null)
                return;

            var guardian = await _unitOfWork.Guardians.GetByIdAsync(guardianId);

            if (guardian == null)
                return;

            var school = await _unitOfWork.Schools.GetByIdAsync(teacher.SchoolId);

            if (school == null)
                return;

            bool canCreate = false;

            switch (type)
            {
                case UserType.SchoolOwner:
                    canCreate = (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id == school.SchoolOwnerId;
                    break;

                case UserType.SchoolAdmin:
                    canCreate = (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.SchoolId == school.Id;
                    break;

                case UserType.Guardian:
                    {
                        var currentGuardian = await _unitOfWork.Guardians.GetByUserIdAsync(userId);

                        if (currentGuardian == null || currentGuardian.Id != guardianId)
                            return;


                        var studentIds = (await _unitOfWork.GuardianStudents
                            .GetByGuardianIdAsync(currentGuardian.Id))
                            .Select(gs => gs.StudentId);

                        if (!studentIds.Any())
                            return;

                        var classGroupIds = (await _unitOfWork.ClassGroupStudents
                            .GetAllAsync(cgs => studentIds.Contains(cgs.StudentId)))
                            .Select(cgs => cgs.ClassGroupId);

                        if (!classGroupIds.Any())
                            return;

                        var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                            .GetAllAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId)))
                            .Select(cgs => cgs.Id);

                        var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                            .AnyAsync(ca => classGroupSubjectIds.Contains(ca.ClassGroupSubjectId) && ca.TeacherId == teacherId);

                        var isHomeTeacher = await _unitOfWork.ClassGroups
                            .AnyAsync(cg => classGroupIds.Contains(cg.Id) &&
                            cg.HomeroomTeacherId == teacherId);

                        canCreate = isAssignedToClassGroup || isHomeTeacher;

                        break;
                    }
                case UserType.Teacher:
                    {
                        var currentTeacher = await _unitOfWork.Teachers.GetByUserIdAsync(userId);
                        if (currentTeacher == null || currentTeacher.Id != teacherId)
                            return;

                        var studentIds = (await _unitOfWork.GuardianStudents
                            .GetByGuardianIdAsync(guardian.Id))
                            .Select(gs => gs.StudentId);

                        if (!studentIds.Any())
                            return;

                        var classGroupIds = (await _unitOfWork.ClassGroupStudents
                            .GetAllAsync(cgs => studentIds.Contains(cgs.StudentId)))
                            .Select(cgs => cgs.ClassGroupId);

                        if (!classGroupIds.Any())
                            return;

                        var classGroupSubjectIds = (await _unitOfWork.ClassGroupsSubjects
                            .GetAllAsync(cgs => classGroupIds.Contains(cgs.ClassGroupId)))
                            .Select(cgs => cgs.Id);

                        var isAssignedToClassGroup = await _unitOfWork.ClassAssignments
                            .AnyAsync(ca => classGroupSubjectIds.Contains(ca.ClassGroupSubjectId) && ca.TeacherId == currentTeacher.Id);

                        var isHomeTeacher = await _unitOfWork.ClassGroups
                            .AnyAsync(cg => classGroupIds.Contains(cg.Id) &&
                            cg.HomeroomTeacherId == currentTeacher.Id);

                        canCreate = isAssignedToClassGroup || isHomeTeacher;

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
