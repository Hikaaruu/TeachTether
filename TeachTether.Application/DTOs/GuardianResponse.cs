using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs;

public class GuardianResponse
{
    public int Id { get; set; }
    public required UserDto User { get; set; }
    public int SchoolId { get; set; }
    public DateOnly DateOfBirth { get; set; }
}