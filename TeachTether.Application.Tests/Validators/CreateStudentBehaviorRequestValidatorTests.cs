using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateStudentBehaviorRequestValidatorTests
{
    private readonly CreateStudentBehaviorRequestValidator _sut = new();

    private static CreateStudentBehaviorRequest CreateValidRequest() => new()
    {
        SubjectId = 1,
        BehaviorDate = DateOnly.FromDateTime(DateTime.Today),
        BehaviorScore = 7.50m,
        Comment = null
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
    public void Should_NotHaveError_When_Comment_IsNull()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Comment = null;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }

    // --- SubjectId ---

    [Fact]
    public void Should_HaveError_When_SubjectId_IsZero()
    {
        // Arrange
        var model = CreateValidRequest();
        model.SubjectId = 0;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SubjectId);
    }

    [Fact]
    public void Should_HaveError_When_SubjectId_IsNegative()
    {
        // Arrange
        var model = CreateValidRequest();
        model.SubjectId = -1;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SubjectId);
    }

    // --- BehaviorDate ---

    [Fact]
    public void Should_HaveError_When_BehaviorDate_IsDefaultValue()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorDate = DateOnly.MinValue;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BehaviorDate);
    }

    [Fact]
    public void Should_HaveError_When_BehaviorDate_IsInTheFuture()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BehaviorDate);
    }

    [Fact]
    public void Should_NotHaveError_When_BehaviorDate_IsToday()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BehaviorDate);
    }

    // --- BehaviorScore ---

    [Fact]
    public void Should_HaveError_When_BehaviorScore_IsBelowMinimum()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorScore = -0.01m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BehaviorScore);
    }

    [Fact]
    public void Should_HaveError_When_BehaviorScore_ExceedsMaximum()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorScore = 10.01m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BehaviorScore);
    }

    [Fact]
    public void Should_HaveError_When_BehaviorScore_ExceedsPrecisionScale()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorScore = 5.001m; // 3 decimal places, exceeds scale 2

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BehaviorScore);
    }

    [Fact]
    public void Should_NotHaveError_When_BehaviorScore_IsAtMinBoundary()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorScore = 0.00m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BehaviorScore);
    }

    [Fact]
    public void Should_NotHaveError_When_BehaviorScore_IsAtMaxBoundary()
    {
        // Arrange
        var model = CreateValidRequest();
        model.BehaviorScore = 10.00m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BehaviorScore);
    }

    // --- Comment (conditional: only validated when non-null) ---

    [Fact]
    public void Should_HaveError_When_Comment_IsNonNull_AndEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Comment = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Should_HaveError_When_Comment_IsNonNull_AndExceedsMaxLength()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Comment = new string('a', 451);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Should_HaveError_When_Comment_IsNonNull_AndContainsInvalidCharacters()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Comment = "Comment\nwith newline";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Should_NotHaveError_When_Comment_IsNonNull_AndValid()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Comment = "Student was disruptive.";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }
}
