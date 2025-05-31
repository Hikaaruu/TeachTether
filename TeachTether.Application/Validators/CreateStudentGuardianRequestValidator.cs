using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateStudentGuardianRequestValidator : AbstractValidator<CreateStudentGuardianRequest>
{
    public CreateStudentGuardianRequestValidator()
    {
        RuleFor(x => x.GuardianId)
            .GreaterThan(0)
            .WithMessage("GuardianId must be a positive integer.");
    }
}