using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateStudentGuardianRequestValidatorTests
{
    private readonly CreateStudentGuardianRequestValidator _sut = new();

    [Fact]
    public void Should_NotHaveErrors_When_GuardianId_IsValid()
    {
        // Arrange
        var model = new CreateStudentGuardianRequest { GuardianId = 1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_GuardianId_IsZero()
    {
        // Arrange
        var model = new CreateStudentGuardianRequest { GuardianId = 0 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GuardianId);
    }

    [Fact]
    public void Should_HaveError_When_GuardianId_IsNegative()
    {
        // Arrange
        var model = new CreateStudentGuardianRequest { GuardianId = -1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GuardianId);
    }
}
