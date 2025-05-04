using FluentValidation;
using TeachTether.Application.Common.Models;

namespace TeachTether.Application.Validators
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().MaximumLength(50).Matches("^[A-Za-z' -]+$");

            RuleFor(x => x.LastName)
                .NotEmpty().MaximumLength(50).Matches("^[A-Za-z' -]+$");

            RuleFor(x => x.MiddleName)
               .NotEmpty().MaximumLength(50).Matches("^[A-Za-z' -]+$");

            RuleFor(x => x.Sex)
                .Must(s => s == 'M' || s == 'F').WithMessage("Sex must be 'M' or 'F'");
        }
    }
}
