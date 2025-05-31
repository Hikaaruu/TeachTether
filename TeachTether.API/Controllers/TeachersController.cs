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
public class TeachersController(
    ITeacherService teacherService,
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    IGuardianService guardianService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IGuardianService _guardianService = guardianService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly ITeacherService _teacherService = teacherService;

    [HttpGet]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetAllBySchool(int schoolId)
    {
        var _ = await _schoolService.GetByIdAsync(schoolId);

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var teachers = await _teacherService.GetAllBySchoolAsync(schoolId);

        return Ok(teachers);
    }

    [HttpGet("/api/guardians/me/teachers")]
    [Authorize(Policy = "RequireGuardian")]
    public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetAvailableForGuardian()
    {
        var guardianIdStr = User.FindFirstValue("entity_id");
        if (string.IsNullOrWhiteSpace(guardianIdStr) || !int.TryParse(guardianIdStr, out var guardianId))
            return Forbid();
        _ = await _guardianService.GetByIdAsync(guardianId);

        var teachers = await _teacherService.GetAvailableForGuardianAsync(guardianId);

        return Ok(teachers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeacherResponse>> Get(int id, int schoolId)
    {
        var teacher = await _teacherService.GetByIdAsync(id);
        if (teacher.SchoolId != schoolId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new CanViewTeacherRequirement());

        if (!authResult.Succeeded)
            return Forbid();

        return Ok(teacher);
    }

    [HttpPost]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<ActionResult<CreatedTeacherResponse>> Create(int schoolId,
        [FromBody] CreateTeacherRequest request)
    {
        var _ = await _schoolService.GetByIdAsync(schoolId);

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var createdTeacher = await _teacherService.CreateAsync(request, schoolId);
        return CreatedAtAction(nameof(Get), new { id = createdTeacher.Id, schoolId }, createdTeacher);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> Update(int schoolId, int id, [FromBody] UpdateTeacherRequest request)
    {
        var teacher = await _teacherService.GetByIdAsync(id);
        if (teacher.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _teacherService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> Delete(int schoolId, int id)
    {
        var teacher = await _teacherService.GetByIdAsync(id);
        if (teacher.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _teacherService.DeleteAsync(id);
        return NoContent();
    }
}