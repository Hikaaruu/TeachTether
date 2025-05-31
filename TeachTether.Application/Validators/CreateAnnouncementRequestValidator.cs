using FluentValidation;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Validators;

public class CreateAnnouncementRequestValidator : AbstractValidator<CreateAnnouncementRequest>
{
    private const string SafeTextPattern = @"^[\p{L}\p{N}\p{P}\p{Zs}]+$";

    private static readonly string AllowedAudiences =
        string.Join(", ", Enum.GetNames<GradeType>());

    public CreateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(90)
            .WithMessage("Title cannot exceed 90 characters.")
            .Matches(SafeTextPattern)
            .WithMessage("Title contains invalid characters.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(9_000)
            .WithMessage("Message is too long.")
            .Matches(SafeTextPattern)
            .WithMessage("Message contains invalid characters.");

        RuleFor(x => x.TargetAudience)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("TargetAudience is required")
            .IsEnumName(typeof(AudienceType), false)
            .WithMessage($"Grade type must be one of: {AllowedAudiences}.");

        RuleFor(x => x.ClassGroupIds)
            .NotNull()
            .WithMessage("ClassGroupIds are required.")
            .Must(list => list.Count > 0)
            .WithMessage("At least one ClassGroupId must be provided.");

        RuleForEach(x => x.ClassGroupIds)
            .GreaterThan(0)
            .WithMessage("ClassGroupId must be a positive integer.");
    }
}