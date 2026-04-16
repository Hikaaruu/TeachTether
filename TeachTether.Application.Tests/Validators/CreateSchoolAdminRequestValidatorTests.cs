using FluentValidation.TestHelper;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateSchoolAdminRequestValidatorTests
{
    private readonly CreateSchoolAdminRequestValidator _sut =
        new(new CreateUserDtoValidator());

    private static CreateUserDto CreateValidUserDto() => new()
    {
        Email = "admin@school.com",
        FirstName = "Jane",
        LastName = "Smith",
        MiddleName = null,
        Sex = 'F'
    };

    private static CreateSchoolAdminRequest CreateValidRequest() => new()
    {
        User = CreateValidUserDto()
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
    public void Should_HaveError_When_User_FirstName_IsEmpty()
    {
        // Arrange
        var model = CreateValidRequest();
        model.User.FirstName = "";

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor("User.FirstName");
    }

    [Fact]
    public void Should_HaveError_When_User_Sex_IsInvalid()
    {
        // Arrange
        var model = CreateValidRequest();
        model.User.Sex = 'X';

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor("User.Sex");
    }
}
