namespace TeachTether.Application.DTOs
{
    public class CreatedTeacherResponse : TeacherResponse
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
