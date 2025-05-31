using FluentValidation;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class UpdateSchoolAdminRequestValidator : AbstractValidator<UpdateSchoolAdminRequest>
{
    public UpdateSchoolAdminRequestValidator(IValidator<UpdateUserDto> userValidator)
    {
        RuleFor(x => x.User).SetValidator(userValidator);
    }
}