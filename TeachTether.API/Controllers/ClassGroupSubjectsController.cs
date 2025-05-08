using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/classgroups/{classGroupId}/subjects")]
    [ApiController]
    [Authorize]
    public class ClassGroupSubjectsController : ControllerBase
    {
        private readonly IClassGroupService _classGroupService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IClassGroupSubjectService _classGroupSubjectService;
        private readonly ISubjectService _subjectService;

        public ClassGroupSubjectsController(IClassGroupService classGroupService, ISchoolService schoolService, IAuthorizationService authorizationService, IClassGroupSubjectService classGroupSubjectService, ISubjectService subjectService)
        {
            _classGroupService = classGroupService;
            _authorizationService = authorizationService;
            _schoolService = schoolService;
            _classGroupSubjectService = classGroupSubjectService;
            _subjectService = subjectService;
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Create(int schoolId, int classGroupId, [FromBody] CreateClassGroupSubjectRequest request)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var subject = await _subjectService.GetByIdAsync(request.SubjectId);
            if (classGroup.SchoolId != schoolId || subject.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classGroupSubjectService.CreateAsync(request, classGroupId);
            return NoContent();
        }

        [HttpDelete("{subjectId}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int classGroupId, int subjectId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (classGroup.SchoolId != schoolId || subject.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classGroupSubjectService.DeleteAsync(classGroupId, subjectId);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectResponse>>> GetSubjects(int classGroupId, int schoolId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            if (classGroup.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, classGroupId, new CanViewClassGroupSubjectsRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var subjects = await _subjectService.GetAllByClassGroupAsync(classGroupId);

            return Ok(subjects);
        }

    }
}
