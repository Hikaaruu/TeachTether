using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class UpdateAnnouncementRequestValidatorTests
{
    private readonly UpdateAnnouncementRequestValidator _sut = new();

    private static UpdateAnnouncementRequest CreateValidRequest() => new()
    {
        Title = "Updated Announcement Title",
        Message = "This is the updated announcement message."
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

    // --- Title ---

    [Fact]
    public void Should_HaveError_When_Title_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Title = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_HaveError_When_Title_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Title = new string('a', 91);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_HaveError_When_Title_ContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Title = "Title<WithTags>";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    // --- Message ---

    [Fact]
    public void Should_HaveError_When_Message_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Message = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Should_HaveError_When_Message_ExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Message = new string('a', 9_001);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Should_HaveError_When_Message_ContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Message = "Message\nwith newlines";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }
}
