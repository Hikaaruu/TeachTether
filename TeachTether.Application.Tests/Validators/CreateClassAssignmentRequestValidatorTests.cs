using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateClassAssignmentRequestValidatorTests
{
    private readonly CreateClassAssignmentRequestValidator _sut = new();

    [Fact]
    public void Should_NotHaveErrors_When_TeacherId_IsValid()
    {
        // Arrange
        var model = new CreateClassAssignmentRequest { TeacherId = 1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_TeacherId_IsZero()
    {
        // Arrange
        var model = new CreateClassAssignmentRequest { TeacherId = 0 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TeacherId);
    }

    [Fact]
    public void Should_HaveError_When_TeacherId_IsNegative()
    {
        // Arrange
        var model = new CreateClassAssignmentRequest { TeacherId = -1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TeacherId);
    }
}
