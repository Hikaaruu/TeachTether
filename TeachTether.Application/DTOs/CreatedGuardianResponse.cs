namespace TeachTether.Application.DTOs;

public class CreatedGuardianResponse : GuardianResponse
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}