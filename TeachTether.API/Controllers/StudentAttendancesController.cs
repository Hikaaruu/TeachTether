using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachTether.Application.Authorization.Requirements;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.API.Controllers;

[Route("api/schools/{schoolId}/students/{studentId}/attendance")]
[ApiController]
[Authorize]
public class StudentAttendancesController(
    ISchoolService schoolService,
    IAuthorizationService authorizationService,
    ITeacherService teacherService,
    IStudentService studentService,
    ISubjectService subjectService,
    IStudentAttendanceService studentAttendanceService) : ControllerBase
{
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ISchoolService _schoolService = schoolService;
    private readonly IStudentAttendanceService _studentAttendanceService = studentAttendanceService;
    private readonly IStudentService _studentService = studentService;
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ITeacherService _teacherService = teacherService;

    [HttpGet("/api/schools/{schoolId}/students/{studentId}/attendance/subjects/{subjectId}")]
    public async Task<ActionResult<IEnumerable<StudentAttendanceResponse>>> GetAllByStudent(int studentId, int schoolId,
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

        var studentAttendances = await _studentAttendanceService.GetAllByStudentAsync(studentId, subjectId);

        return Ok(studentAttendances);
    }

    [HttpGet("{attendanceId}")]
    public async Task<ActionResult<StudentAttendanceResponse>> Get(int studentId, int schoolId, int attendanceId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var attendance = await _studentAttendanceService.GetByIdAsync(attendanceId);
        if (student.SchoolId != schoolId ||
            attendance.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, attendance.SubjectId),
            new CanViewStudentRecordRequirement());

        if (!authResult.Succeeded)
            return Forbid();

        return Ok(attendance);
    }

    [HttpPost]
    [Authorize(Policy = "RequireTeacher")]
    public async Task<ActionResult<StudentAttendanceResponse>> Create(int studentId, int schoolId,
        [FromBody] CreateStudentAttendanceRequest request)
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

        var studentAttendance = await _studentAttendanceService.CreateAsync(request, teacherId, studentId);
        return CreatedAtAction(nameof(Get), new { attendanceId = studentAttendance.Id, studentId, schoolId },
            studentAttendance);
    }

    [HttpPut("{attendanceId}")]
    [Authorize(Policy = "RequireSchoolOwnerAdminOrTeacher")]
    public async Task<IActionResult> Update(int studentId, int schoolId, int attendanceId,
        [FromBody] UpdateStudentAttendanceRequest request)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var attendance = await _studentAttendanceService.GetByIdAsync(attendanceId);

        if (student.SchoolId != schoolId ||
            attendance.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, attendance.SubjectId),
            new CanModifyStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _studentAttendanceService.UpdateAsync(attendanceId, request);
        return NoContent();
    }

    [HttpDelete("{attendanceId}")]
    [Authorize(Policy = "RequireSchoolOwnerAdminOrTeacher")]
    public async Task<IActionResult> Delete(int studentId, int schoolId, int attendanceId)
    {
        _ = await _schoolService.GetByIdAsync(schoolId);
        var student = await _studentService.GetByIdAsync(studentId);
        var attendance = await _studentAttendanceService.GetByIdAsync(attendanceId);

        if (student.SchoolId != schoolId ||
            attendance.StudentId != studentId)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, (studentId, attendance.SubjectId),
            new CanModifyStudentRecordsRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _studentAttendanceService.DeleteAsync(attendanceId);
        return NoContent();
    }
}