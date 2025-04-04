namespace TeachTether.Domain.Entities
{
    public class StudentAttendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public AttendanceStatus Status { get; set; } 
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }
}
