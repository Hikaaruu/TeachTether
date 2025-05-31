using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateClassGroupSubjectRequestValidator : AbstractValidator<CreateClassGroupSubjectRequest>
{
    public CreateClassGroupSubjectRequestValidator()
    {
        RuleFor(x => x.SubjectId)
            .GreaterThan(0)
            .WithMessage("SubjectId must be a positive integer.");
    }
}