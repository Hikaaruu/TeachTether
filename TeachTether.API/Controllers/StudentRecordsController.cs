using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentRecordsController : ControllerBase
    {
        private readonly IClassGroupService _classGroupService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISubjectService _subjectService;
        private readonly IStudentRecordsService _studentRecordsService;

        public StudentRecordsController(IClassGroupService classGroupService, ISchoolService schoolService, IAuthorizationService authorizationService, ISubjectService subjectService, IStudentRecordsService studentRecordsService)
        {
            _classGroupService = classGroupService;
            _authorizationService = authorizationService;
            _schoolService = schoolService;
            _subjectService = subjectService;
            _studentRecordsService = studentRecordsService;
        }

        [HttpDelete("classgroups/{classGroupId}/subjects/{subjectId}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> DeleteForClassGroupAndSubject(int classGroupId, int subjectId, int schoolId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (classGroup.SchoolId != schoolId || subject.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _studentRecordsService.DeleteForClassGroupSubject(classGroupId, subjectId);
            return NoContent();
        }

    }
}
