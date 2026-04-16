using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();

    private static RegisterRequest CreateValidRequest() => new()
    {
        UserName = "validuser",
        Email = "user@example.com",
        Password = "ValidPass1!",
        FirstName = "John",
        LastName = "Doe",
        MiddleName = null,
        Sex = 'M'
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
    public void Should_NotHaveErrors_When_MiddleName_IsNull()
    {
        // Arrange
        var model = CreateValidRequest();
        model.MiddleName = null;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MiddleName);
    }

    // --- UserName ---

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
    public void Should_HaveError_When_UserName_IsBelowMinLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.UserName = "abc"; // 3 chars, min is 4

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
    public void Should_HaveError_When_UserName_ContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.UserName = "user@name!";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    // --- Email ---

    [Fact]
    public void Should_HaveError_When_Email_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Email = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_When_Email_HasInvalidFormat()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Email = "not-an-email";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_When_Email_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Email = new string('a', 92) + "@test.com"; // 101 chars, exceeds max of 100

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // --- Password ---

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
    public void Should_HaveError_When_Password_IsBelowMinLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = "Ab1!"; // 4 chars, min is 6

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

    [Fact]
    public void Should_HaveError_When_Password_HasNoUppercaseLetter()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = "validpass1!";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_Password_HasNoLowercaseLetter()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = "VALIDPASS1!";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_Password_HasNoDigit()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = "ValidPass!!";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveError_When_Password_HasNoNonAlphanumericCharacter()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Password = "ValidPass123";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    // --- FirstName ---

    [Fact]
    public void Should_HaveError_When_FirstName_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.FirstName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_HaveError_When_FirstName_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.FirstName = new string('a', 51);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_HaveError_When_FirstName_ContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.FirstName = "John123";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    // --- LastName ---

    [Fact]
    public void Should_HaveError_When_LastName_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.LastName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_HaveError_When_LastName_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.LastName = new string('a', 51);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_HaveError_When_LastName_ContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.LastName = "Doe@123";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    // --- MiddleName (conditional: only validated when non-null) ---

    [Fact]
    public void Should_HaveError_When_MiddleName_IsNonNull_AndEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.MiddleName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MiddleName);
    }

    [Fact]
    public void Should_HaveError_When_MiddleName_IsNonNull_AndExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.MiddleName = new string('a', 51);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MiddleName);
    }

    [Fact]
    public void Should_HaveError_When_MiddleName_IsNonNull_AndContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.MiddleName = "Middle123";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MiddleName);
    }

    // --- Sex ---

    [Fact]
    public void Should_HaveError_When_Sex_IsInvalid()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Sex = 'X';

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sex);
    }
}
