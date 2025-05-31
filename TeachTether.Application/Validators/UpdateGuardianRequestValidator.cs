using FluentValidation;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class UpdateGuardianRequestValidator : AbstractValidator<UpdateGuardianRequest>
{
    public UpdateGuardianRequestValidator(IValidator<UpdateUserDto> userValidator)
    {
        RuleFor(x => x.User).SetValidator(userValidator);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddYears(-16)))
            .WithMessage("Must be at least 16 years old.");
    }
}