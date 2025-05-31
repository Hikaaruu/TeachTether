using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/classgroups/{classGroupId}/subjects")]
[ApiController]
[Authorize]
public class ClassGroupSubjectsController(
    IClassGroupService classGroupService,
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    IClassGroupSubjectService classGroupSubjectService,
    ISubjectService subjectService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IClassGroupService _classGroupService = classGroupService;
    private readonly IClassGroupSubjectService _classGroupSubjectService = classGroupSubjectService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly ISubjectService _subjectService = subjectService;

    [HttpPost]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> Create(int schoolId, int classGroupId,
        [FromBody] CreateClassGroupSubjectRequest request)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        var subject = await _subjectService.GetByIdAsync(request.SubjectId);
        if (classGroup.SchoolId != schoolId || subject.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _classGroupSubjectService.CreateAsync(request, classGroupId);
        return NoContent();
    }

    [HttpDelete("{subjectId}")]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> Delete(int schoolId, int classGroupId, int subjectId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        var subject = await _subjectService.GetByIdAsync(subjectId);
        if (classGroup.SchoolId != schoolId || subject.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, schoolId, new CanManageSchoolEntitiesRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _classGroupSubjectService.DeleteAsync(classGroupId, subjectId);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubjectResponse>>> GetSubjects(int classGroupId, int schoolId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        if (classGroup.SchoolId != schoolId)
            return NotFound();

        var authResult =
            await _authorizationService.AuthorizeAsync(User, classGroupId, new CanViewClassGroupSubjectsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var subjects = await _subjectService.GetAllByClassGroupAsync(classGroupId);

        return Ok(subjects);
    }
}