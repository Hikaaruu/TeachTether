namespace TeachTether.Application.DTOs
{
    public class CreateStudentGradeRequest
    {
        public int SubjectId { get; set; }
        public decimal GradeValue { get; set; }
        public required string GradeType { get; set; }
        public string? Comment { get; set; }
        public DateOnly GradeDate { get; set; }
    }
}
