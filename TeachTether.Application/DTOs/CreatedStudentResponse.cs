namespace TeachTether.Application.DTOs
{
    public class CreatedStudentResponse : StudentResponse
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
