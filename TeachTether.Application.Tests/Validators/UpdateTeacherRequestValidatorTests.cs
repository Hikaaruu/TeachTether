using FluentValidation.TestHelper;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class UpdateTeacherRequestValidatorTests
{
    private readonly UpdateTeacherRequestValidator _sut =
        new(new UpdateUserDtoValidator());

    private static UpdateUserDto CreateValidUserDto() => new()
    {
        FirstName = "Robert",
        LastName = "Brown",
        MiddleName = null,
        Sex = 'M'
    };

    private static UpdateTeacherRequest CreateValidRequest() => new()
    {
        User = CreateValidUserDto(),
        DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30))
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
        model.DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-18));

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
    public void Should_HaveError_When_DateOfBirth_IsUnder18()
    {
        // Arrange
        var model = CreateValidRequest();
        model.DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-17));

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
        model.User.LastName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor("User.LastName");
    }
}
