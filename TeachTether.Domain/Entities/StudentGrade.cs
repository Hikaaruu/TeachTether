namespace TeachTether.Domain.Entities;

public class StudentGrade
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int SubjectId { get; set; }
    public decimal GradeValue { get; set; }
    public GradeType GradeType { get; set; }
    public string? Comment { get; set; }
    public DateOnly GradeDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum GradeType
{
    Exam,
    Quiz,
    Homework,
    Project,
    LabWork,
    OralPresentation,
    ClassParticipation,
    ExtraCredit,
    PracticeWork,
    FinalGrade,
    Other
}