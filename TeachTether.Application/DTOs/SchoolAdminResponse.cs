using TeachTether.Domain.Entities;

namespace TeachTether.Application.DTOs
{
    public class SchoolAdminResponse
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public Sex Sex { get; set; }
        public int SchoolId { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}
