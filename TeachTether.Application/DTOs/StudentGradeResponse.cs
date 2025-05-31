namespace TeachTether.Application.DTOs;

public class StudentGradeResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int SubjectId { get; set; }
    public decimal GradeValue { get; set; }
    public required string GradeType { get; set; }
    public string? Comment { get; set; }
    public DateOnly GradeDate { get; set; }

    public required string TeacherName { get; set; }
}