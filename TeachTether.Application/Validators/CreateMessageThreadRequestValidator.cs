using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateMessageThreadRequestValidator : AbstractValidator<CreateMessageThreadRequest>
{
    public CreateMessageThreadRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0)
            .WithMessage("TeacherId must be a positive integer.");

        RuleFor(x => x.GuardianId)
            .GreaterThan(0)
            .WithMessage("GuardianId must be a positive integer.");
    }
}