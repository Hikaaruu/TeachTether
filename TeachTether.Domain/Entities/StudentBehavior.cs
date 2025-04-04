namespace TeachTether.Domain.Entities
{
    public class StudentBehavior
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public decimal BehaviorScore { get; set; }
        public string? Comment { get; set; }
        public DateOnly BehaviorDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
