using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class UpdateStudentBehaviorRequestValidator : AbstractValidator<UpdateStudentBehaviorRequest>
{
    public UpdateStudentBehaviorRequestValidator()
    {
        RuleFor(x => x.BehaviorScore)
            .InclusiveBetween(0.00m, 10.00m)
            .WithMessage("Behavior score must be between 0.00 and 10.00.")
            .PrecisionScale(4, 2, false)
            .WithMessage("Behavior score must have at most 2 decimal places and 4 total digits.");


        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment can not be empty")
            .MaximumLength(450)
            .WithMessage("Comment cannot exceed 450 characters.")
            .Matches(@"^[\p{L}\p{N}\p{P}\p{Zs}]*$")
            .WithMessage("Comment may contain only letters, numbers, punctuation, and spaces.")
            .When(x => x.Comment is not null);
    }
}