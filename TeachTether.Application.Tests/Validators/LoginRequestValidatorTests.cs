using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    private static LoginRequest CreateValidRequest() => new()
    {
        UserName = "validuser",
        Password = "ValidPass1!"
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
    public void Should_HaveError_When_UserName_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.UserName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Should_HaveError_When_UserName_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.UserName = new string('a', 51);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Should_HaveError_When_Password_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_Password_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = new string('a', 101);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
