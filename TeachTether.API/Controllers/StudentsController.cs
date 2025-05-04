using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;

        public StudentsController(IStudentService studentService, ISchoolService schoolService, IAuthorizationService authorizationService)
        {
            _studentService = studentService;
            _authorizationService = authorizationService;
            _schoolService = schoolService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetAllBySchool(int schoolId)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var students = await _studentService.GetAllBySchoolAsync(schoolId);

            return Ok(students);

        }

        [HttpGet("/api/schools/{schoolId}/classgroups/{classGroupId}/students")]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetAllByClassGroup(int schoolId, int classGroupId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, classGroupId, new CanViewClassGroupStudentsRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var students = await _studentService.GetAllByClassGroupAsync(classGroupId);

            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentResponse>> Get(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewStudentRequirement());

            if (!authResult.Succeeded)
                return Forbid();

            return Ok(student);
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<CreatedStudentResponse>> Create(int schoolId, [FromBody] CreateStudentRequest request)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var createdStudent = await _studentService.CreateAsync(request, schoolId);
            return CreatedAtAction(nameof(Get), new { id = createdStudent.Id }, createdStudent);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Update(int schoolId, int id, [FromBody] UpdateStudentRequest request)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _studentService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]

        public async Task<IActionResult> Delete(int schoolId, int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _studentService.DeleteAsync(id);
            return NoContent();
        }

    }
}
