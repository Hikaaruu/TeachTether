using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/[controller]")]
[ApiController]
[Authorize]
public class AnalyticsController(
    IAnalyticsService analyticsService,
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    IStudentService studentService,
    ISubjectService subjectService,
    IClassGroupService classGroupService) : ControllerBase
{
    private readonly IAnalyticsService _analyticsService = analyticsService;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IClassGroupService _classGroupService = classGroupService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentService _studentService = studentService;
    private readonly ISubjectService _subjectService = subjectService;

    [HttpGet("classgroups/{classGroupId}/subjects/{subjectId}/students/{studentId}/averages")]
    public async Task<ActionResult<ClassAveragesResponse>> Get(int schoolId, int classGroupId, int subjectId,
        int studentId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var subject = await _subjectService.GetByIdAsync(subjectId);
        var classGroup = await _classGroupService.GetByIdAsync(classGroupId);
        if (student.SchoolId != schoolId ||
            subject.SchoolId != schoolId ||
            classGroup.SchoolId != schoolId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, subjectId),
            new CanViewRecordsOfStudentRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var averages = await _analyticsService.GetClassAverages(subjectId, studentId, classGroupId);

        return Ok(averages);
    }
}