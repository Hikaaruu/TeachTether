using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateGuardianStudentRequestValidatorTests
{
    private readonly CreateGuardianStudentRequestValidator _sut = new();

    [Fact]
    public void Should_NotHaveErrors_When_StudentId_IsValid()
    {
        // Arrange
        var model = new CreateGuardianStudentRequest { StudentId = 1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_StudentId_IsZero()
    {
        // Arrange
        var model = new CreateGuardianStudentRequest { StudentId = 0 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StudentId);
    }

    [Fact]
    public void Should_HaveError_When_StudentId_IsNegative()
    {
        // Arrange
        var model = new CreateGuardianStudentRequest { StudentId = -1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StudentId);
    }
}
