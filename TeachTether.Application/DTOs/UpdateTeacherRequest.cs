using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs;

public class UpdateTeacherRequest
{
    public required UpdateUserDto User { get; set; }
    public DateOnly DateOfBirth { get; set; }
}