namespace TeachTether.Application.DTOs
{
    public class UserInfoResponse
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Email { get; set; }
        public required string Sex { get; set; }
        public required string Role { get; set; }
        public int EntityId { get; set; }
    }
}
