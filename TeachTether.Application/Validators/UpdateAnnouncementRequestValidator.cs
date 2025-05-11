using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators
{
    public class UpdateAnnouncementRequestValidator : AbstractValidator<UpdateAnnouncementRequest>
    {
        private const string SafeTextPattern = @"^[\p{L}\p{N}\p{P}\p{Zs}]+$";

        public UpdateAnnouncementRequestValidator()
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
        }
    }
}
