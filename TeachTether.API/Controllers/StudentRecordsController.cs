using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/[controller]")]
[ApiController]
[Authorize]
public class StudentRecordsController(
    IClassGroupService classGroupService,
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    ISubjectService subjectService,
    IStudentRecordsService studentRecordsService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IClassGroupService _classGroupService = classGroupService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentRecordsService _studentRecordsService = studentRecordsService;
    private readonly ISubjectService _subjectService = subjectService;

    [HttpDelete("classgroups/{classGroupId}/subjects/{subjectId}")]
    [Authorize(Policy = "RequireSchoolOwnerOrAdmin")]
    public async Task<IActionResult> DeleteForClassGroupAndSubject(int classGroupId, int subjectId, int schoolId)
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

        await _studentRecordsService.DeleteForClassGroupSubject(classGroupId, subjectId);
        return NoContent();
    }
}