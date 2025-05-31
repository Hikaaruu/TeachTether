namespace TeachTether.Application.Common.Models;

public class UpdateUserDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public required char Sex { get; set; }
}