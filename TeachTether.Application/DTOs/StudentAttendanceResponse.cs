namespace TeachTether.Application.DTOs
{
    public class StudentAttendanceResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public required string Status { get; set; }
        public string? Comment { get; set; }
    }
}
