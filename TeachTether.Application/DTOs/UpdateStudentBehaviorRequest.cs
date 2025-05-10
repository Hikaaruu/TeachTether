namespace TeachTether.Application.DTOs
{
    public class UpdateStudentBehaviorRequest
    {
        public decimal BehaviorScore { get; set; }
        public string? Comment { get; set; }
    }
}
