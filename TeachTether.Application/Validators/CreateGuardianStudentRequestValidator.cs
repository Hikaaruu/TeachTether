using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateGuardianStudentRequestValidator : AbstractValidator<CreateGuardianStudentRequest>
{
    public CreateGuardianStudentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("StudentId must be a positive integer.");
    }
}