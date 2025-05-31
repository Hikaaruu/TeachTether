using TeachTether.Application.Common.Models;

namespace TeachTether.Application.DTOs;

public class CreateTeacherRequest
{
    public required CreateUserDto User { get; set; }
    public DateOnly DateOfBirth { get; set; }
}