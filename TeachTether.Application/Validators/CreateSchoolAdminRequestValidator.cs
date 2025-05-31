using FluentValidation;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateSchoolAdminRequestValidator : AbstractValidator<CreateSchoolAdminRequest>
{
    public CreateSchoolAdminRequestValidator(IValidator<CreateUserDto> userValidator)
    {
        RuleFor(x => x.User).SetValidator(userValidator);
    }
}