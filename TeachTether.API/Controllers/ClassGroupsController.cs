using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/[controller]")]
[ApiController]
[Authorize]
public class ClassGroupsController(
    IClassGroupService classGroupService,
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    ITeacherService teacherService,
    IStudentService studentService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IClassGroupService _classGroupService = classGroupService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentService _studentService = studentService;
    private readonly ITeacherService _teacherService = teacherService;

    [HttpGet]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<ActionResult<IEnumerable<ClassGroupResponse>>> GetAllBySchool(int schoolId)
    {
        var _ = await _schoolService.GetByIdAsync(schoolId);

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var classGroups = await _classGroupService.GetAllBySchoolAsync(schoolId);

        return Ok(classGroups);
    }

    [HttpGet("/api/teachers/me/classgroups")]
    [Authorize(Policy = "RequireTeacher")]
    public async Task<ActionResult<IEnumerable<ClassGroupResponse>>> GetAvailableForTeacher(int schoolId)
    {
        var teacherIdStr = User.FindFirstValue("entity_id");
        if (string.IsNullOrWhiteSpace(teacherIdStr) || !int.TryParse(teacherIdStr, out var teacherId))
            return Forbid();
        _ = await _teacherService.GetByIdAsync(teacherId);

        var classGroups = await _classGroupService.GetAvailableForTeacherAsync(teacherId);

        return Ok(classGroups);
    }

    [HttpGet("/api/teachers/me/homeroom-classgroups")]
    [Authorize(Policy = "RequireTeacher")]
    public async Task<ActionResult<IEnumerable<ClassGroupResponse>>> GetAllByTeacher()
    {
        var teacherIdStr = User.FindFirstValue("entity_id");
        if (string.IsNullOrWhiteSpace(teacherIdStr) || !int.TryParse(teacherIdStr, out var teacherId))
            return Forbid();
        _ = await _teacherService.GetByIdAsync(teacherId);

        var classGroups = await _classGroupService.GetAllByTeacherAsync(teacherId);

        return Ok(classGroups);
    }

    [HttpGet("/api/schools/{schoolId}/students/{studentId}/classgroup")]
    public async Task<ActionResult<ClassGroupResponse>> GetByStudent(int schoolId, int studentId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        if (student.SchoolId != schoolId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, studentId, new CanViewStudentRequirement());

        if (!authResult.Succeeded)
            return Forbid();

        var classGroup = await _classGroupService.GetByStudentAsync(studentId);

        return Ok(classGroup);
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
        var teacher = await _teacherService.GetByIdAsync(request.HomeroomTeacherId);
        if (teacher.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
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

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
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

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _classGroupService.DeleteAsync(id);
        return NoContent();
    }
}