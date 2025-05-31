using FluentValidation;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator(IValidator<CreateUserDto> userValidator)
    {
        RuleFor(x => x.User).SetValidator(userValidator);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddYears(-5)))
            .WithMessage("Must be at least 5 years old.");
    }
}