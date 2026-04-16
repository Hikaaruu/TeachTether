using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateStudentAttendanceRequestValidatorTests
{
    private readonly CreateStudentAttendanceRequestValidator _sut = new();

    private static CreateStudentAttendanceRequest CreateValidRequest() => new()
    {
        SubjectId = 1,
        AttendanceDate = DateOnly.FromDateTime(DateTime.Today),
        Status = "Present",
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

    // --- AttendanceDate ---

    [Fact]
    public void Should_HaveError_When_AttendanceDate_IsDefaultValue()
    {
        // Arrange
        var model = CreateValidRequest();
        model.AttendanceDate = DateOnly.MinValue; // DateOnly default = empty

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AttendanceDate);
    }

    [Fact]
    public void Should_HaveError_When_AttendanceDate_IsInTheFuture()
    {
        // Arrange
        var model = CreateValidRequest();
        model.AttendanceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AttendanceDate);
    }

    [Fact]
    public void Should_NotHaveError_When_AttendanceDate_IsToday()
    {
        // Arrange
        var model = CreateValidRequest();
        model.AttendanceDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AttendanceDate);
    }

    // --- Status ---

    [Fact]
    public void Should_HaveError_When_Status_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Status = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_HaveError_When_Status_IsNotValidEnumName()
    {
        // Arrange
        var model = CreateValidRequest();
        model.Status = "Flying";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData("Present")]
    [InlineData("Absent")]
    [InlineData("Late")]
    [InlineData("Excused")]
    public void Should_NotHaveError_When_Status_IsValidEnumName(string status)
    {
        // Arrange
        var model = CreateValidRequest();
        model.Status = status;

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
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
        model.Comment = "Student was sick.";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }
}
