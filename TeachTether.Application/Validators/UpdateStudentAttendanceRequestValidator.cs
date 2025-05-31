using FluentValidation;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Validators;

public class UpdateStudentAttendanceRequestValidator : AbstractValidator<UpdateStudentAttendanceRequest>
{
    private static readonly string AllowedStatuses =
        string.Join(", ", Enum.GetNames<AttendanceStatus>());

    public UpdateStudentAttendanceRequestValidator()
    {
        RuleFor(x => x.Status)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => Enum.TryParse<AttendanceStatus>(s, true, out _))
            .WithMessage(_ => $"Status must be one of: {AllowedStatuses}.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment can not be empty")
            .MaximumLength(450)
            .WithMessage("Comment cannot exceed 450 characters.")
            .Matches(@"^[\p{L}\p{N}\p{P}\p{Zs}]*$")
            .WithMessage("Comment may contain only letters, numbers, punctuation, and spaces.")
            .When(x => x.Comment is not null);
    }
}