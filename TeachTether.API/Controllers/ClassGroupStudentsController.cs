using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/classgroups/{classGroupId}/students")]
    [ApiController]
    [Authorize]
    public class ClassGroupStudentsController : ControllerBase
    {
        private readonly IClassGroupService _classGroupService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStudentService _studentService;
        private readonly IClassGroupStudentService _classGroupStudentService;

        public ClassGroupStudentsController(IClassGroupService classGroupService, ISchoolService schoolService, IAuthorizationService authorizationService, IStudentService studentService, IClassGroupStudentService classGroupStudentService)
        {
            _classGroupService = classGroupService;
            _authorizationService = authorizationService;
            _schoolService = schoolService;
            _studentService = studentService;
            _classGroupStudentService = classGroupStudentService;
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Create(int schoolId,int classGroupId, [FromBody] CreateClassGroupStudentRequest request)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var student = await _studentService.GetByIdAsync(request.StudentId);
            if (classGroup.SchoolId != schoolId || student.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classGroupStudentService.CreateAsync(request, classGroupId);
            return NoContent();
        }

        [HttpDelete("{studentId}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int classGroupId, int studentId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var student = await _studentService.GetByIdAsync(studentId);
            if (classGroup.SchoolId != schoolId || student.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classGroupStudentService.DeleteAsync(classGroupId,studentId);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetStudents(int classGroupId, int schoolId)
        {
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            if (classGroup.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, classGroupId, new CanViewClassGroupStudentsRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var students = await _studentService.GetAllByClassGroupAsync(classGroupId);

            return Ok(students);
        }
    }
}
