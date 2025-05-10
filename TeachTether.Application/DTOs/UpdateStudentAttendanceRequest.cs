namespace TeachTether.Application.DTOs
{
    public class UpdateStudentAttendanceRequest
    {
        public required string Status { get; set; }
        public string? Comment { get; set; }
    }
}
