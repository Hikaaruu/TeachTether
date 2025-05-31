using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs;

public class UpdateGuardianRequest
{
    public required UpdateUserDto User { get; set; }
    public DateOnly DateOfBirth { get; set; }
}