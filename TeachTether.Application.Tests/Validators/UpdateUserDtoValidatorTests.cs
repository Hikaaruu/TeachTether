using FluentValidation.TestHelper;
using TeachTether.Application.Common.Models;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _sut = new();

    private static UpdateUserDto CreateValidDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        MiddleName = null,
        Sex = 'F'
    };

    [Fact]
    public void Should_NotHaveErrors_When_Model_IsValid()
    {
        // Arrange
        var model = CreateValidDto();

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveErrors_When_MiddleName_IsNull()
    {
        // Arrange
        var model = CreateValidDto();
        model.MiddleName = null;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MiddleName);
    }

    // --- FirstName ---

    [Fact]
    public void Should_HaveError_When_FirstName_IsEmpty()
    {
        // Arrange
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
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
        var model = CreateValidDto();
        model.Sex = 'X';

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sex);
    }
}
