using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class UpdateClassGroupRequestValidatorTests
{
    private readonly UpdateClassGroupRequestValidator _sut = new();

    private static UpdateClassGroupRequest CreateValidRequest() => new()
    {
        GradeYear = 5,
        Section = 'B',
        HomeroomTeacherId = 2
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
    public void Should_NotHaveError_When_Section_IsCyrillicUppercase()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Section = 'Я'; // Cyrillic Я (U+042F)

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Section);
    }

    // --- GradeYear ---

    [Fact]
    public void Should_HaveError_When_GradeYear_IsZero()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeYear = 0;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeYear);
    }

    [Fact]
    public void Should_HaveError_When_GradeYear_ExceedsMaximum()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeYear = 13;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeYear);
    }

    [Fact]
    public void Should_NotHaveError_When_GradeYear_IsAtMinBoundary()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeYear = 1;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GradeYear);
    }

    [Fact]
    public void Should_NotHaveError_When_GradeYear_IsAtMaxBoundary()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeYear = 12;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GradeYear);
    }

    // --- Section ---

    [Fact]
    public void Should_HaveError_When_Section_IsLowercaseLatin()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Section = 'b';

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Section);
    }

    [Fact]
    public void Should_HaveError_When_Section_IsDigit()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Section = '7';

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Section);
    }

    [Fact]
    public void Should_HaveError_When_Section_IsSpace()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Section = ' ';

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Section);
    }

    // --- HomeroomTeacherId ---

    [Fact]
    public void Should_HaveError_When_HomeroomTeacherId_IsZero()
    {
        // Arrange
        var model = CreateValidRequest();
        model.HomeroomTeacherId = 0;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HomeroomTeacherId);
    }

    [Fact]
    public void Should_HaveError_When_HomeroomTeacherId_IsNegative()
    {
        // Arrange
        var model = CreateValidRequest();
        model.HomeroomTeacherId = -1;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HomeroomTeacherId);
    }
}
