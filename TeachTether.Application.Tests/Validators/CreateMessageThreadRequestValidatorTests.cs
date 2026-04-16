using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateMessageThreadRequestValidatorTests
{
    private readonly CreateMessageThreadRequestValidator _sut = new();

    private static CreateMessageThreadRequest CreateValidRequest() => new()
    {
        TeacherId = 1,
        GuardianId = 2
    };

    [Fact]
    public void Should_NotHaveErrors_When_Model_IsValid()
    {
        // Arrange
        var model = CreateValidRequest();

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- TeacherId ---

    [Fact]
    public void Should_HaveError_When_TeacherId_IsZero()
    {
        // Arrange
        var model = CreateValidRequest();
        model.TeacherId = 0;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TeacherId);
    }

    [Fact]
    public void Should_HaveError_When_TeacherId_IsNegative()
    {
        // Arrange
        var model = CreateValidRequest();
        model.TeacherId = -1;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TeacherId);
    }

    // --- GuardianId ---

    [Fact]
    public void Should_HaveError_When_GuardianId_IsZero()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GuardianId = 0;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GuardianId);
    }

    [Fact]
    public void Should_HaveError_When_GuardianId_IsNegative()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GuardianId = -1;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GuardianId);
    }
}
