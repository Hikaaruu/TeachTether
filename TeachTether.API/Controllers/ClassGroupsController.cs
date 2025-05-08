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
    public class ClassGroupsController : ControllerBase
    {
        private readonly IClassGroupService _classGroupService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStudentService _studentService;

        public ClassGroupsController(IClassGroupService classGroupService, ISchoolService schoolService, IAuthorizationService authorizationService, IStudentService studentService)
        {
            _classGroupService = classGroupService;
            _authorizationService = authorizationService;
            _schoolService = schoolService;
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<IEnumerable<ClassGroupResponse>>> GetAllBySchool(int schoolId)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var classGroups = await _classGroupService.GetAllBySchoolAsync(schoolId);

            return Ok(classGroups);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassGroupResponse>> Get(int id, int schoolId)
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);
            if (classGroup.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewClassGroupRequirement());

            if (!authResult.Succeeded)
                return Forbid();

            return Ok(classGroup);
        }

        [HttpPost]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<ActionResult<ClassGroupResponse>> Create(int schoolId, [FromBody] CreateClassGroupRequest request)
        {
            var _ = await _schoolService.GetByIdAsync(schoolId);

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var classGroup = await _classGroupService.CreateAsync(request, schoolId);
            return CreatedAtAction(nameof(Get), new { id = classGroup.Id, schoolId }, classGroup);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Update(int schoolId, int id, [FromBody] UpdateClassGroupRequest request)
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);
            if (classGroup.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classGroupService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int schoolId, int id)
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);
            if (classGroup.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            await _classGroupService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/students")]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetStudents(int id, int schoolId)
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);
            if (classGroup.SchoolId != schoolId)
                return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewClassGroupStudentsRequirement());
            if (!authResult.Succeeded)
                return Forbid();

            var students = await _studentService.GetAllByClassGroupAsync(id);

            return Ok(students);
        }
    }
}
