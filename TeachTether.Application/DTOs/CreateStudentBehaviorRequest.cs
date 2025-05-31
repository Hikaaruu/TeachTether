namespace TeachTether.Application.DTOs;

public class CreateStudentBehaviorRequest
{
    public int SubjectId { get; set; }
    public decimal BehaviorScore { get; set; }
    public string? Comment { get; set; }
    public DateOnly BehaviorDate { get; set; }
}