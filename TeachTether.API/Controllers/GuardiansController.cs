using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/[controller]")]
    [ApiController]
    [Authorize]
    public class GuardiansController : ControllerBase
    {
        private readonly IGuardianService _guardianService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITeacherService _teacherService;

        public GuardiansController(IGuardianService guardianService, ISchoolService schoolService, IAuthorizationService authorizationService, ITeacherService teacherService)
        {
            _guardianService = guardianService;
            _schoolService = schoolService;
            _authorizationService = authorizationService;
            _teacherService = teacherService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<IEnumerable<GuardianResponse>>> GetAllBySchool(int schoolId)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var guardians = await _guardianService.GetAllBySchoolAsync(schoolId);

            return Ok(guardians);
        }

        [HttpGet("/api/teachers/me/guardians")]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<ActionResult<IEnumerable<GuardianResponse>>> GetAvailableForTeacher()
        {
            var teacherIdStr = User.FindFirstValue("entity_id");
            if (string.IsNullOrWhiteSpace(teacherIdStr) || !int.TryParse(teacherIdStr, out int teacherId))
                return Forbid();

            var teacher = await _teacherService.GetByIdAsync(teacherId);

            var guardians = await _guardianService.GetAvailableForTeacherAsync(teacherId);

            return Ok(guardians);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GuardianResponse>> Get(int id, int schoolId)
        {
            var guardian = await _guardianService.GetByIdAsync(id);
            if (guardian.SchoolId != schoolId)
                return NotFound();
            
            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewGuardianRequirement());

            if (!authResult.Succeeded)
                return Forbid();

            return Ok(guardian);
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<CreatedGuardianResponse>> Create(int schoolId, [FromBody] CreateGuardianRequest request)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var createdGuardian = await _guardianService.CreateAsync(request, schoolId);
            return CreatedAtAction(nameof(Get), new { id = createdGuardian.Id, schoolId }, createdGuardian);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Update(int schoolId, int id, [FromBody] UpdateGuardianRequest request)
        {
            var guardian = await _guardianService.GetByIdAsync(id);
            if (guardian.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _guardianService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int id)
        {
            var guardian = await _guardianService.GetByIdAsync(id);
            if (guardian.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _guardianService.DeleteAsync(id);
            return NoContent();
        }
    }
}
