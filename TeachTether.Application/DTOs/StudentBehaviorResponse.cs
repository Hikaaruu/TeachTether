namespace TeachTether.Application.DTOs
{
    public class StudentBehaviorResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public decimal BehaviorScore { get; set; }
        public string? Comment { get; set; }
        public DateOnly BehaviorDate { get; set; }

        public required string TeacherName { get; set; }
    }
}
