using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/classgroups/{classGroupId}/students")]
[ApiController]
[Authorize]
public class ClassGroupStudentsController(
    IClassGroupService classGroupService,
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    IStudentService studentService,
    IClassGroupStudentService classGroupStudentService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IClassGroupService _classGroupService = classGroupService;
    private readonly IClassGroupStudentService _classGroupStudentService = classGroupStudentService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentService _studentService = studentService;

    [HttpPost]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> Create(int schoolId, int classGroupId,
        [FromBody] CreateClassGroupStudentRequest request)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        var student = await _studentService.GetByIdAsync(request.StudentId);
        if (classGroup.SchoolId != schoolId || student.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _classGroupStudentService.CreateAsync(request, classGroupId);
        return NoContent();
    }

    [HttpDelete("{studentId}")]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> Delete(int schoolId, int classGroupId, int studentId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        var student = await _studentService.GetByIdAsync(studentId);
        if (classGroup.SchoolId != schoolId || student.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _classGroupStudentService.DeleteAsync(classGroupId, studentId);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentResponse>>> GetStudents(int classGroupId, int schoolId)
    {
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        if (classGroup.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, classGroupId, new CanViewClassGroupStudentsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var students = await _studentService.GetAllByClassGroupAsync(classGroupId);

        return Ok(students);
    }
}