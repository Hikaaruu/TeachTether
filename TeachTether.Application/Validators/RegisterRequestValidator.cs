using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(4).WithMessage("Username must be at least 4 characters long.")
                .MaximumLength(50).WithMessage("Username cannot be longer than 50 characters.")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username contains invalid characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.")
                .MaximumLength(100).WithMessage("Email cannot be longer than 100 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(100).WithMessage("Password cannot be longer than 100 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one digit.")
                .Matches(@"\W").WithMessage("Password must contain at least one non-alphanumeric character.");

            RuleFor(x => x.FirstName)
                .NotEmpty().MaximumLength(50).Matches("^[A-Za-z' -]+$");

            RuleFor(x => x.LastName)
                .NotEmpty().MaximumLength(50).Matches("^[A-Za-z' -]+$");

            RuleFor(x => x.MiddleName)
                .NotEmpty().MaximumLength(50).Matches("^[A-Za-z' -]+$");

            RuleFor(x => x.Sex)
                .Must(s => s == 'M' || s == 'F').WithMessage("Sex must be 'M' or 'F'");
        }
    }
}
