namespace TeachTether.Application.DTOs;

public class CreateStudentAttendanceRequest
{
    public int SubjectId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public required string Status { get; set; }
    public string? Comment { get; set; }
}