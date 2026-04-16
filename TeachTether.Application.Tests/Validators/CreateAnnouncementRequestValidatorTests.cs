using FluentAssertions;
using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateAnnouncementRequestValidatorTests
{
    private readonly CreateAnnouncementRequestValidator _sut = new();

    private static CreateAnnouncementRequest CreateValidRequest() => new()
    {
        Title = "School Announcement",
        Message = "This is an important message for all students.",
        TargetAudience = "Student",
        ClassGroupIds = [1, 2]
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

    // --- TargetAudience ---

    [Fact]
    public void Should_HaveError_When_TargetAudience_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.TargetAudience = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TargetAudience);
    }

    [Fact]
    public void Should_HaveError_When_TargetAudience_IsNotValidEnumName()
    {
        // Arrange
        var model = CreateValidRequest();
        model.TargetAudience = "Teachers";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TargetAudience);
    }

    [Theory]
    [InlineData("Student")]
    [InlineData("Guardian")]
    [InlineData("StudentAndGuardian")]
    public void Should_NotHaveError_When_TargetAudience_IsValidEnumName(string audience)
    {
        // Arrange
        var model = CreateValidRequest();
        model.TargetAudience = audience;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TargetAudience);
    }

    // --- ClassGroupIds ---

    [Fact]
    public void Should_HaveError_When_ClassGroupIds_IsEmptyList()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ClassGroupIds = [];

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClassGroupIds);
    }

    [Fact]
    public void Should_HaveError_When_ClassGroupIds_ContainsZero()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ClassGroupIds = [0];

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "ClassGroupIds[0]");
    }

    [Fact]
    public void Should_HaveError_When_ClassGroupIds_ContainsNegativeValue()
    {
        // Arrange
        var model = CreateValidRequest();
        model.ClassGroupIds = [-5];

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "ClassGroupIds[0]");
    }
}
