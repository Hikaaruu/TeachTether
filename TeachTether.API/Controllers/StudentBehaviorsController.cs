using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/students/{studentId}/behavior")]
[ApiController]
[Authorize]
public class StudentBehaviorsController(
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    ITeacherService teacherService,
    IStudentService studentService,
    ISubjectService subjectService,
    IStudentBehaviorService studentBehaviorService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentBehaviorService _studentBehaviorService = studentBehaviorService;
    private readonly IStudentService _studentService = studentService;
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ITeacherService _teacherService = teacherService;

    [HttpGet("/api/schools/{schoolId}/students/{studentId}/behavior/subjects/{subjectId}")]
    public async Task<ActionResult<IEnumerable<StudentBehaviorResponse>>> GetAllByStudent(int studentId, int schoolId,
        int subjectId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var subject = await _subjectService.GetByIdAsync(subjectId);
        if (student.SchoolId != schoolId || subject.SchoolId != schoolId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, subjectId),
            new CanViewRecordsOfStudentRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var studentBehaviors = await _studentBehaviorService.GetAllByStudentAsync(studentId, subjectId);

        return Ok(studentBehaviors);
    }

    [HttpGet("{behaviorId}")]
    public async Task<ActionResult<StudentBehaviorResponse>> Get(int studentId, int schoolId, int behaviorId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var behavior = await _studentBehaviorService.GetByIdAsync(behaviorId);
        if (student.SchoolId != schoolId ||
            behavior.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, behavior.SubjectId),
            new CanViewStudentRecordRequirement());

        if (!authResult.Succeeded)
            return Forbid();

        return Ok(behavior);
    }

    [HttpPost]
    [Authorize(Policy = "RequireTeacher")]
    public async Task<ActionResult<StudentBehaviorResponse>> Create(int studentId, int schoolId,
        [FromBody] CreateStudentBehaviorRequest request)
    {
        var teacherId = int.Parse(User.FindFirstValue("entity_id")!);

        var school = await _schoolService.GetByIdAsync(schoolId);
        var teacher = await _teacherService.GetByIdAsync(teacherId);
        var student = await _studentService.GetByIdAsync(studentId);
        var subject = await _subjectService.GetByIdAsync(request.SubjectId);

        if (student.SchoolId != schoolId ||
            teacher.SchoolId != schoolId ||
            subject.SchoolId != schoolId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, request.SubjectId),
            new CanCreateStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var studentBehavior = await _studentBehaviorService.CreateAsync(request, teacherId, studentId);
        return CreatedAtAction(nameof(Get), new { behaviorId = studentBehavior.Id, studentId, schoolId },
            studentBehavior);
    }

    [HttpPut("{behaviorId}")]
    [Authorize(Policy = "RequireSchoolOwnerAdminOrTeacher")]
    public async Task<IActionResult> Update(int studentId, int schoolId, int behaviorId,
        [FromBody] UpdateStudentBehaviorRequest request)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var behavior = await _studentBehaviorService.GetByIdAsync(behaviorId);

        if (student.SchoolId != schoolId ||
            behavior.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, behavior.SubjectId),
            new CanModifyStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _studentBehaviorService.UpdateAsync(behaviorId, request);
        return NoContent();
    }

    [HttpDelete("{behaviorId}")]
    [Authorize(Policy = "RequireSchoolOwnerAdminOrTeacher")]
    public async Task<IActionResult> Delete(int studentId, int schoolId, int behaviorId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var behavior = await _studentBehaviorService.GetByIdAsync(behaviorId);

        if (student.SchoolId != schoolId ||
            behavior.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, behavior.SubjectId),
            new CanModifyStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _studentBehaviorService.DeleteAsync(behaviorId);
        return NoContent();
    }
}