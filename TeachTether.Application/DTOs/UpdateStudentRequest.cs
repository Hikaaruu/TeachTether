namespace TeachTether.Application.DTOs
{
    public class UpdateStudentRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Email { get; set; }
        public char Sex { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
