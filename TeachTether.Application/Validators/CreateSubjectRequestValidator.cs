using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MinimumLength(3).WithMessage("Subject name must be at least 3 characters long.")
            .MaximumLength(180).WithMessage("Subject name cannot be longer than 180 characters.")
            .Matches(@"^[\p{L}\p{N}\s\-_'&.,]+$").WithMessage("Subject name contains invalid characters.");
    }
}