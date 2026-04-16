using FluentValidation.TestHelper;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateStudentRequestValidatorTests
{
    private readonly CreateStudentRequestValidator _sut =
        new(new CreateUserDtoValidator());

    private static CreateUserDto CreateValidUserDto() => new()
    {
        Email = "student@school.com",
        FirstName = "Alice",
        LastName = "Johnson",
        MiddleName = null,
        Sex = 'F'
    };

    private static CreateStudentRequest CreateValidRequest() => new()
    {
        User = CreateValidUserDto(),
        DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-10))
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
    public void Should_NotHaveError_When_DateOfBirth_IsExactlyAtMinAge()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-5));

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Should_HaveError_When_DateOfBirth_IsToday()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DateOfBirth = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Should_HaveError_When_DateOfBirth_IsUnder5()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-4));

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Should_HaveError_When_User_IsInvalid()
    {
        // Arrange
        var model = CreateValidRequest();
        model.User.FirstName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor("User.FirstName");
    }
}
