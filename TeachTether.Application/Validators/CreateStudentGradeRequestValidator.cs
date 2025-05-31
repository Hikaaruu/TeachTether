using FluentValidation;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Validators;

public class CreateStudentGradeRequestValidator : AbstractValidator<CreateStudentGradeRequest>
{
    private static readonly string AllowedGradeTypes =
        string.Join(", ", Enum.GetNames<GradeType>());

    public CreateStudentGradeRequestValidator()
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0)
            .WithMessage("SubjectId must be a positive integer.");

        RuleFor(x => x.GradeDate)
            .NotEmpty()
            .WithMessage("GradeDate is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("GradeDate cannot be in the future");

        RuleFor(x => x.GradeValue)
            .InclusiveBetween(0.00m, 100.00m)
            .WithMessage("Grade value must be between 0.00 and 100.00.")
            .PrecisionScale(5, 2, false)
            .WithMessage("Grade value must have at most 2 decimal places and 5 total digits.");

        RuleFor(x => x.GradeType)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Grade type is required")
            .IsEnumName(typeof(GradeType), false)
            .WithMessage($"Grade type must be one of: {AllowedGradeTypes}.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment can not be empty")
            .MaximumLength(450)
            .WithMessage("Comment cannot exceed 450 characters.")
            .Matches(@"^[\p{L}\p{N}\p{P}\p{Zs}]*$")
            .WithMessage("Comment may contain only letters, numbers, punctuation, and spaces.")
            .When(x => x.Comment is not null);
    }
}