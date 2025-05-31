namespace TeachTether.Application.DTOs;

public class CreatedSchoolAdminResponse : SchoolAdminResponse
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}