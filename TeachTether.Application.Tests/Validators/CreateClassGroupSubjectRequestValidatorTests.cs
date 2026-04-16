using FluentValidation.TestHelper;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateClassGroupSubjectRequestValidatorTests
{
    private readonly CreateClassGroupSubjectRequestValidator _sut = new();

    [Fact]
    public void Should_NotHaveErrors_When_SubjectId_IsValid()
    {
        // Arrange
        var model = new CreateClassGroupSubjectRequest { SubjectId = 1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_SubjectId_IsZero()
    {
        // Arrange
        var model = new CreateClassGroupSubjectRequest { SubjectId = 0 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SubjectId);
    }

    [Fact]
    public void Should_HaveError_When_SubjectId_IsNegative()
    {
        // Arrange
        var model = new CreateClassGroupSubjectRequest { SubjectId = -1 };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SubjectId);
    }
}
