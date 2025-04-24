namespace TeachTether.Application.Common.Models
{
    public class CreateUserDto
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }
        public char Sex { get; set; }
    }
}
