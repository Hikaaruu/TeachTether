using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}")]
    [ApiController]
    [Authorize]
    public class GuardianStudentsController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStudentService _studentService;
        private readonly IGuardianService _guardianService;
        private readonly IGuardianStudentService _guardianStudentService;

        public GuardianStudentsController(IAuthorizationService authorizationService, ISchoolService schoolService, IStudentService studentService, IGuardianService guardianService, IGuardianStudentService guardianStudentService)
        {
            _authorizationService = authorizationService;
            _schoolService = schoolService;
            _studentService = studentService;
            _guardianService = guardianService;
            _guardianStudentService = guardianStudentService;
        }

        [HttpPost("students/{studentId}/guardians")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Create(int schoolId, int studentId, [FromBody] CreateStudentGuardianRequest request)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var student = await _studentService.GetByIdAsync(studentId);
            var guardian = await _guardianService.GetByIdAsync(request.GuardianId);
            if (student.SchoolId != schoolId || guardian.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _guardianStudentService.CreateAsync(studentId, request.GuardianId);
            return NoContent();
        }

        [HttpPost("guardians/{guardianId}/students")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Create(int schoolId, int guardianId, [FromBody] CreateGuardianStudentRequest request)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var student = await _studentService.GetByIdAsync(request.StudentId);
            var guardian = await _guardianService.GetByIdAsync(guardianId);
            if (student.SchoolId != schoolId || guardian.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _guardianStudentService.CreateAsync(request.StudentId, guardianId);
            return NoContent();
        }

        [HttpDelete("students/{studentId}/guardians/{guardianId}")]
        [HttpDelete("guardians/{guardianId}/students/{studentId}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int studentId, int guardianId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var student = await _studentService.GetByIdAsync(studentId);
            var guardian = await _guardianService.GetByIdAsync(guardianId);
            if (student.SchoolId != schoolId || guardian.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _guardianStudentService.DeleteAsync(studentId,guardianId);
            return NoContent();
        }

        [HttpGet("guardians/{guardianId}/students")]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetStudents(int schoolId, int guardianId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var guardian = await _guardianService.GetByIdAsync(guardianId);
            if (guardian.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, guardianId, new CanViewGuardianStudentsRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var students = await _studentService.GetAllByGuardianAsync(guardianId);

            return Ok(students);
        }

        [HttpGet("students/{studentId}/guardians")]
        public async Task<ActionResult<IEnumerable<GuardianResponse>>> GetGuardians(int schoolId, int studentId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var student = await _studentService.GetByIdAsync(studentId);
            if (student.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, studentId, new CanViewStudentGuardiansRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var guardians = await _guardianService.GetAllByStudentAsync(studentId);

            return Ok(guardians);
        }
    }
}
