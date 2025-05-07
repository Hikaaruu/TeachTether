using FluentValidation;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators
{
    public class CreateTeacherRequestValidator : AbstractValidator<CreateTeacherRequest>
    {
        public CreateTeacherRequestValidator(IValidator<CreateUserDto> userValidator)
        {
            RuleFor(x => x.User).SetValidator(userValidator);

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddYears(-18)))
                .WithMessage("Must be at least 18 years old.");
        }
    }
}
