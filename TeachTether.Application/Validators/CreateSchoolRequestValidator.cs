using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateSchoolRequestValidator : AbstractValidator<CreateSchoolRequest>
{
    public CreateSchoolRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required.")
            .MinimumLength(3).WithMessage("School name must be at least 3 characters long.")
            .MaximumLength(180).WithMessage("School name cannot be longer than 180 characters.")
            .Matches(@"^[\p{L}\p{N}\s\-_'\&.,]+$").WithMessage("School name contains invalid characters.");
    }
}