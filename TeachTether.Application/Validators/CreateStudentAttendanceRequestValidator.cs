using FluentValidation;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Validators;

public class CreateStudentAttendanceRequestValidator : AbstractValidator<CreateStudentAttendanceRequest>
{
    private static readonly string AllowedStatuses =
        string.Join(", ", Enum.GetNames<AttendanceStatus>());

    public CreateStudentAttendanceRequestValidator()
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0)
            .WithMessage("SubjectId must be a positive integer.");

        RuleFor(x => x.AttendanceDate)
            .NotEmpty()
            .WithMessage("AttendanceDate is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("AttendanceDate cannot be in the future");

        RuleFor(x => x.Status)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Status is required")
            .IsEnumName(typeof(AttendanceStatus), false)
            .WithMessage($"Status must be one of: {AllowedStatuses}.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment can not be empty")
            .MaximumLength(450)
            .WithMessage("Comment cannot exceed 450 characters.")
            .Matches(@"^[\p{L}\p{N}\p{P}\p{Zs}]*$")
            .WithMessage("Comment may contain only letters, numbers, punctuation, and spaces.")
            .When(x => x.Comment is not null);
    }
}