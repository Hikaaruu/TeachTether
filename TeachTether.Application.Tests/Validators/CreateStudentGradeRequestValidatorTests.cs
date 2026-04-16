using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateStudentGradeRequestValidatorTests
{
    private readonly CreateStudentGradeRequestValidator _sut = new();

    private static CreateStudentGradeRequest CreateValidRequest() => new()
    {
        SubjectId = 1,
        GradeDate = DateOnly.FromDateTime(DateTime.Today),
        GradeValue = 85.50m,
        GradeType = "Exam",
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

    // --- GradeDate ---

    [Fact]
    public void Should_HaveError_When_GradeDate_IsDefaultValue()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeDate = DateOnly.MinValue;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeDate);
    }

    [Fact]
    public void Should_HaveError_When_GradeDate_IsInTheFuture()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeDate);
    }

    [Fact]
    public void Should_NotHaveError_When_GradeDate_IsToday()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GradeDate);
    }

    // --- GradeValue ---

    [Fact]
    public void Should_HaveError_When_GradeValue_IsBelowMinimum()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeValue = -0.01m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeValue);
    }

    [Fact]
    public void Should_HaveError_When_GradeValue_ExceedsMaximum()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeValue = 100.01m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeValue);
    }

    [Fact]
    public void Should_HaveError_When_GradeValue_ExceedsPrecisionScale()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeValue = 85.001m; // 3 decimal places, exceeds scale 2

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeValue);
    }

    [Fact]
    public void Should_NotHaveError_When_GradeValue_IsAtMinBoundary()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeValue = 0.00m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GradeValue);
    }

    [Fact]
    public void Should_NotHaveError_When_GradeValue_IsAtMaxBoundary()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeValue = 100.00m;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GradeValue);
    }

    // --- GradeType ---

    [Fact]
    public void Should_HaveError_When_GradeType_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeType = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeType);
    }

    [Fact]
    public void Should_HaveError_When_GradeType_IsNotValidEnumName()
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeType = "MysteryGrade";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GradeType);
    }

    [Theory]
    [InlineData("Exam")]
    [InlineData("Quiz")]
    [InlineData("Homework")]
    [InlineData("Project")]
    [InlineData("FinalGrade")]
    public void Should_NotHaveError_When_GradeType_IsValidEnumName(string gradeType)
    {
        // Arrange
        var model = CreateValidRequest();
        model.GradeType = gradeType;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GradeType);
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
        model.Comment = "Good work on the exam.";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }
}
