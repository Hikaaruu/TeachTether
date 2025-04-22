namespace TeachTether.Application.DTOs
{
    public class CreateSchoolAdminRequest
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }
        public char Sex { get; set; }
    }
}
