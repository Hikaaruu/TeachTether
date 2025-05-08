using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;

        public SubjectsController(ISubjectService subjectService, ISchoolService schoolService, IAuthorizationService authorizationService)
        {
            _subjectService = subjectService;
            _schoolService = schoolService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<IEnumerable<SubjectResponse>>> GetAllBySchool(int schoolId)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var subjects = await _subjectService.GetAllBySchoolAsync(schoolId);

            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectResponse>> Get(int id, int schoolId)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewSubjectRequirement());

            if (!authResult.Succeeded)
                return Forbid();

            return Ok(subject);
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<SubjectResponse>> Create(int schoolId, [FromBody] CreateSubjectRequest request)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var subject = await _subjectService.CreateAsync(request, schoolId);
            return CreatedAtAction(nameof(Get), new { id = subject.Id, schoolId }, subject);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Update(int schoolId, int id, [FromBody] UpdateSubjectRequest request)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _subjectService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _subjectService.DeleteAsync(id);
            return NoContent();
        }
    }
}
