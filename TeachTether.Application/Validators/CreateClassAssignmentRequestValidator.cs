using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateClassAssignmentRequestValidator : AbstractValidator<CreateClassAssignmentRequest>
{
    public CreateClassAssignmentRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0)
            .WithMessage("TeacherId must be a positive integer.");
    }
}