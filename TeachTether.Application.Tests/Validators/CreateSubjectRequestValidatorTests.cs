using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateSubjectRequestValidatorTests
{
    private readonly CreateSubjectRequestValidator _sut = new();

    private static CreateSubjectRequest CreateValidRequest() => new()
    {
        Name = "Mathematics"
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

    [Fact]
    public void Should_HaveError_When_Name_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Name = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_Name_IsBelowMinLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Name = "AB"; // 2 chars, min is 3

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_NotHaveError_When_Name_IsAtMinLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Name = "Art"; // exactly 3 chars

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_Name_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Name = new string('a', 181);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_NotHaveError_When_Name_IsAtMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Name = new string('a', 180);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_Name_ContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Name = "Math@123#";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
