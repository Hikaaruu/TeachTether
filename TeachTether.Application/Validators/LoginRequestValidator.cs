using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(50).WithMessage("Username cannot be longer than 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(100).WithMessage("Password cannot be longer than 100 characters.");
    }
}