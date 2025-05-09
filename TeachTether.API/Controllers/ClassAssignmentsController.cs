using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers
{
    [Route("api/schools/{schoolId}/classgroups/{classGroupId}/subjects{subjectId}/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassAssignmentsController : ControllerBase
    {
        private readonly IClassGroupService _classGroupService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IClassAssignmentService _classAssignmentService;
        private readonly ISubjectService _subjectService;
        private readonly ITeacherService _teacherService;

        public ClassAssignmentsController(IClassGroupService classGroupService, ISchoolService schoolService, IAuthorizationService authorizationService, IClassAssignmentService classAssignmentService, ISubjectService subjectService, ITeacherService teacherService)
        {
            _classGroupService = classGroupService;
            _authorizationService = authorizationService;
            _schoolService = schoolService;
            _classAssignmentService = classAssignmentService;
            _subjectService = subjectService;
            _teacherService = teacherService;
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Create(int schoolId, int classGroupId, int subjectId, [FromBody] CreateClassAssignmentRequest request)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            var teacher = await _teacherService.GetByIdAsync(request.TeacherId);
            if (classGroup.SchoolId != schoolId ||
                subject.SchoolId != schoolId ||
                teacher.SchoolId != schoolId)
            {
                return NotFound();
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classAssignmentService.CreateAsync(request, classGroupId, subjectId);
            return NoContent();
        }

        [HttpDelete("{teacherId}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int classGroupId, int subjectId, int teacherId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            var teacher = await _teacherService.GetByIdAsync(teacherId);
            if (classGroup.SchoolId != schoolId ||
                subject.SchoolId != schoolId ||
                teacher.SchoolId != schoolId)
            {
                return NotFound();
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classAssignmentService.DeleteAsync(classGroupId, subjectId, teacherId);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetTeachers(int schoolId, int classGroupId, int subjectId)
        {
            var school = await _schoolService.GetByIdAsync(schoolId);
            var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
            var subject = await _subjectService.GetByIdAsync(subjectId);
            if (classGroup.SchoolId != schoolId ||
                subject.SchoolId != schoolId)
            {
                return NotFound();
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, classGroupId, new CanViewClassAssignmentsRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var teachers = await _teacherService.GetAllByClassGroupSubjectAsync(classGroupId, subjectId);

            return Ok(teachers);
        }

    }
}
