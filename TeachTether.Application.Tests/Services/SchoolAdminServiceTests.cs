using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class SchoolAdminServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ISchoolAdminDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<ISchoolAdminRepository> _mockAdminRepo = new();
    private readonly SchoolAdminService _sut;

    public SchoolAdminServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.SchoolAdmins).Returns(_mockAdminRepo.Object);
        _sut = new SchoolAdminService(_mockMapper.Object, _mockUnitOfWork.Object, _mockUserService.Object,
            _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldCreateUserAndAdminAndReturnResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockUserService.Setup(s => s.CreateAsync(request.User, UserType.SchoolAdmin))
            .ReturnsAsync((user, "P@ssw0rd"));
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(user.UserName);
        result.Password.Should().Be("P@ssw0rd");
        _mockAdminRepo.Verify(r => r.AddAsync(It.Is<SchoolAdmin>(a =>
            a.UserId == user.Id && a.SchoolId == 1)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToSchoolAdminDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteSchoolAdminAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteSchoolAdminAsync(5), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenAdminExists_ShouldReturnSchoolAdminResponse()
    {
        // Arrange
        var admin = CreateValidAdmin();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockAdminRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);
        _mockUserService.Setup(s => s.GetByIdAsync(admin.UserId)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(admin.Id);
        result.SchoolId.Should().Be(admin.SchoolId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAdminDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockAdminRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SchoolAdmin?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("School admin not found");
    }

    // --- GetAllBySchoolAsync ---

    [Fact]
    public async Task GetAllBySchoolAsync_WhenCalled_ShouldReturnAllAdminsWithUserData()
    {
        // Arrange
        var admin = CreateValidAdmin();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockAdminRepo.Setup(r => r.GetBySchoolIdAsync(1)).ReturnsAsync(new List<SchoolAdmin> { admin });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllBySchoolAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(admin.Id);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenAdminExists_ShouldDelegateUserUpdate()
    {
        // Arrange
        var admin = CreateValidAdmin();
        var request = new UpdateSchoolAdminRequest
        {
            User = new UpdateUserDto { FirstName = "Jane", LastName = "Admin", Sex = 'F' }
        };

        _mockAdminRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(admin);
        _mockUserService.Setup(s => s.UpdateAsync(admin.UserId, request.User)).Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockUserService.Verify(s => s.UpdateAsync(admin.UserId, request.User), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenAdminDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockAdminRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SchoolAdmin?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateSchoolAdminRequest
        {
            User = new UpdateUserDto { FirstName = "X", LastName = "Y", Sex = 'M' }
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("School admin not found");
    }

    // --- Factory methods ---

    private static SchoolAdmin CreateValidAdmin() => new()
    {
        Id = 1,
        UserId = "user-001",
        SchoolId = 1
    };

    private static User CreateValidUser() => new()
    {
        Id = "user-001",
        UserName = "admin.one",
        FirstName = "Admin",
        LastName = "One",
        UserType = UserType.SchoolAdmin
    };

    private static UserDto CreateValidUserDto() => new()
    {
        FirstName = "Admin",
        LastName = "One",
        Sex = 'M'
    };

    private static CreateSchoolAdminRequest CreateValidRequest() => new()
    {
        User = new CreateUserDto { FirstName = "Admin", LastName = "One", Sex = 'M' }
    };
}
