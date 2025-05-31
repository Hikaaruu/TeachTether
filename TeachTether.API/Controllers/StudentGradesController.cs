using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/students/{studentId}/grades")]
[ApiController]
[Authorize]
public class StudentGradesController(
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    ITeacherService teacherService,
    IStudentService studentService,
    ISubjectService subjectService,
    IStudentGradeService studentGradeService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentGradeService _studentGradeService = studentGradeService;
    private readonly IStudentService _studentService = studentService;
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ITeacherService _teacherService = teacherService;

    [HttpGet("/api/schools/{schoolId}/students/{studentId}/grades/subjects/{subjectId}")]
    public async Task<ActionResult<IEnumerable<StudentGradeResponse>>> GetAllByStudent(int studentId, int schoolId,
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

        var studentGrades = await _studentGradeService.GetAllByStudentAsync(studentId, subjectId);

        return Ok(studentGrades);
    }

    [HttpGet("{gradeId}")]
    public async Task<ActionResult<StudentGradeResponse>> Get(int studentId, int schoolId, int gradeId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var grade = await _studentGradeService.GetByIdAsync(gradeId);
        if (student.SchoolId != schoolId ||
            grade.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, grade.SubjectId),
            new CanViewStudentRecordRequirement());

        if (!authResult.Succeeded)
            return Forbid();

        return Ok(grade);
    }

    [HttpPost]
    [Authorize(Policy = "RequireTeacher")]
    public async Task<ActionResult<StudentGradeResponse>> Create(int studentId, int schoolId,
        [FromBody] CreateStudentGradeRequest request)
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

        var studentGrade = await _studentGradeService.CreateAsync(request, teacherId, studentId);
        return CreatedAtAction(nameof(Get), new { gradeId = studentGrade.Id, studentId, schoolId }, studentGrade);
    }

    [HttpPut("{gradeId}")]
    [Authorize(Policy = "RequireSchoolOwnerAdminOrTeacher")]
    public async Task<IActionResult> Update(int studentId, int schoolId, int gradeId,
        [FromBody] UpdateStudentGradeRequest request)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var grade = await _studentGradeService.GetByIdAsync(gradeId);

        if (student.SchoolId != schoolId ||
            grade.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, grade.SubjectId),
            new CanModifyStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _studentGradeService.UpdateAsync(gradeId, request);
        return NoContent();
    }

    [HttpDelete("{gradeId}")]
    [Authorize(Policy = "RequireSchoolOwnerAdminOrTeacher")]
    public async Task<IActionResult> Delete(int studentId, int schoolId, int gradeId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var grade = await _studentGradeService.GetByIdAsync(gradeId);

        if (student.SchoolId != schoolId ||
            grade.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, grade.SubjectId),
            new CanModifyStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _studentGradeService.DeleteAsync(gradeId);
        return NoContent();
    }
}