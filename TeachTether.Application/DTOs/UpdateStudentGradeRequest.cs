namespace TeachTether.Application.DTOs
{
    public class UpdateStudentGradeRequest
    {
        public decimal GradeValue { get; set; }
        public required string GradeType { get; set; }
        public string? Comment { get; set; }
    }
}
