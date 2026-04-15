using FluentValidation;
using TeachTether.Application.Common.Models;

namespace TeachTether.Application.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.")
            .MaximumLength(100).WithMessage("Email cannot be longer than 100 characters.")
            .When(x => x.Email is not null);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[\p{L}' \-]+$")
            .WithMessage("First name contains invalid characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[\p{L}' \-]+$")
            .WithMessage("Last name contains invalid characters.");

        RuleFor(x => x.MiddleName)
            .NotEmpty().MaximumLength(50)
            .Matches(@"^[\p{L}' \-]+$")
            .WithMessage("Middle name contains invalid characters.")
            .When(x => x.MiddleName is not null);

        RuleFor(x => x.Sex)
            .Must(s => s == 'M' || s == 'F').WithMessage("Sex must be 'M' or 'F'");
    }
}