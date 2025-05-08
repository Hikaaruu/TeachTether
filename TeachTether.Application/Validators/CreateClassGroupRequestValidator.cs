using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators
{
    public class CreateClassGroupRequestValidator : AbstractValidator<CreateClassGroupRequest>
    {
        public CreateClassGroupRequestValidator()
        {
            RuleFor(x => x.GradeYear)
               .InclusiveBetween(1, 12)
               .WithMessage("Grade year must be between 1 and 12.");

            RuleFor(x => x.Section)
                .Must(c => c is >= 'A' and <= 'Z')
                .WithMessage("Section must be an uppercase letter A–Z.");

            RuleFor(x => x.HomeroomTeacherId)
                .GreaterThan(0)
                .WithMessage("HomeroomTeacherId must be a positive integer.");
        }
    }
}
