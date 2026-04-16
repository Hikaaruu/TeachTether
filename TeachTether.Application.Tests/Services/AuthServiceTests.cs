using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ISchoolOwnerRepository> _mockOwnerRepo = new();
    private readonly Mock<IStudentRepository> _mockStudentRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly Mock<IGuardianRepository> _mockGuardianRepo = new();
    private readonly Mock<ISchoolAdminRepository> _mockAdminRepo = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.SchoolOwners).Returns(_mockOwnerRepo.Object);
        _mockUnitOfWork.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _mockUnitOfWork.Setup(u => u.Guardians).Returns(_mockGuardianRepo.Object);
        _mockUnitOfWork.Setup(u => u.SchoolAdmins).Returns(_mockAdminRepo.Object);
        _sut = new AuthService(_mockUserService.Object, _mockUnitOfWork.Object, _mockMapper.Object);
    }

    // --- RegisterAsync ---

    [Fact]
    public async Task RegisterAsync_WhenRegistrationSucceeds_ShouldCreateSchoolOwnerAndReturnToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            UserName = "owner.user",
            Email = "owner@test.com",
            Password = "Pass123!",
            FirstName = "Owner",
            LastName = "User",
            Sex = 'M'
        };
        var registeredUser = new User { Id = "u1", UserName = "owner.user", FirstName = "Owner", LastName = "User" };
        var result = OperationResult.Success();

        _mockUserService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync((result, registeredUser));
        _mockOwnerRepo.Setup(r => r.AddAsync(It.IsAny<SchoolOwner>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockUserService.Setup(s => s.TryLoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync("jwt-token");

        // Act
        var (operationResult, token) = await _sut.RegisterAsync(request);

        // Assert
        operationResult.Succeeded.Should().BeTrue();
        token.Should().Be("jwt-token");
        _mockOwnerRepo.Verify(r => r.AddAsync(It.Is<SchoolOwner>(o => o.UserId == "u1")), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenRegistrationFails_ShouldReturnFailureAndNotCreateOwner()
    {
        // Arrange
        var request = new RegisterRequest
        {
            UserName = "bad.user",
            Email = "bad@test.com",
            Password = "weak",
            FirstName = "Bad",
            LastName = "User",
            Sex = 'M'
        };
        var failedUser = new User { UserName = "bad.user", FirstName = "Bad", LastName = "User" };
        var failedResult = OperationResult.Failure(["Password too weak"]);

        _mockUserService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync((failedResult, failedUser));

        // Act
        var (operationResult, token) = await _sut.RegisterAsync(request);

        // Assert
        operationResult.Succeeded.Should().BeFalse();
        token.Should().BeNull();
        _mockOwnerRepo.Verify(r => r.AddAsync(It.IsAny<SchoolOwner>()), Times.Never);
    }

    // --- LoginAsync ---

    [Fact]
    public async Task LoginAsync_WhenCalled_ShouldDelegateToUserService()
    {
        // Arrange
        var request = new LoginRequest { UserName = "john.doe", Password = "Pass123!" };
        _mockUserService.Setup(s => s.TryLoginAsync(request)).ReturnsAsync("login-token");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().Be("login-token");
        _mockUserService.Verify(s => s.TryLoginAsync(request), Times.Once);
    }

    // --- GetCurrentUserInfoAsync ---

    [Fact]
    public async Task GetCurrentUserInfoAsync_WhenClaimsMissingUserId_ShouldReturnNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // no NameIdentifier claim

        // Act
        var result = await _sut.GetCurrentUserInfoAsync(principal);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserInfoAsync_WhenUserIsTeacher_ShouldReturnUserInfoWithEntityAndSchoolId()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "u1") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var dbUser = new User { Id = "u1", UserName = "ann.smith", FirstName = "Ann", LastName = "Smith", UserType = UserType.Teacher };
        var teacher = new Teacher { Id = 5, UserId = "u1", SchoolId = 10, DateOfBirth = default };
        var userInfo = new UserInfoResponse
        {
            Id = "u1",
            UserName = "a.smith",
            FirstName = "Ann",
            LastName = "Smith",
            Sex = "F",
            Role = "Teacher"
        };

        _mockUserService.Setup(s => s.GetByIdAsync("u1")).ReturnsAsync(dbUser);
        _mockTeacherRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(teacher);
        _mockMapper.Setup(m => m.Map<UserInfoResponse>(dbUser)).Returns(userInfo);

        // Act
        var result = await _sut.GetCurrentUserInfoAsync(principal);

        // Assert
        result.Should().NotBeNull();
        result!.EntityId.Should().Be(5);
        result.SchoolId.Should().Be(10);
    }

    [Fact]
    public async Task GetCurrentUserInfoAsync_WhenUserIsSchoolOwner_ShouldReturnUserInfoWithNullSchoolId()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "u2") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var dbUser = new User { Id = "u2", UserName = "bob.jones", FirstName = "Bob", LastName = "Jones", UserType = UserType.SchoolOwner };
        var owner = new SchoolOwner { Id = 3, UserId = "u2" };
        var userInfo = new UserInfoResponse
        {
            Id = "u2",
            UserName = "b.jones",
            FirstName = "Bob",
            LastName = "Jones",
            Sex = "M",
            Role = "SchoolOwner"
        };

        _mockUserService.Setup(s => s.GetByIdAsync("u2")).ReturnsAsync(dbUser);
        _mockOwnerRepo.Setup(r => r.GetByUserIdAsync("u2")).ReturnsAsync(owner);
        _mockMapper.Setup(m => m.Map<UserInfoResponse>(dbUser)).Returns(userInfo);

        // Act
        var result = await _sut.GetCurrentUserInfoAsync(principal);

        // Assert
        result.Should().NotBeNull();
        result!.EntityId.Should().Be(3);
        result.SchoolId.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserInfoAsync_WhenEntityIdIsNull_ShouldReturnNull()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "u3") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var dbUser = new User { Id = "u3", UserName = "x.y", FirstName = "X", LastName = "Y", UserType = UserType.Student };

        _mockUserService.Setup(s => s.GetByIdAsync("u3")).ReturnsAsync(dbUser);
        _mockStudentRepo.Setup(r => r.GetByUserIdAsync("u3")).ReturnsAsync((Student?)null);

        // Act
        var result = await _sut.GetCurrentUserInfoAsync(principal);

        // Assert
        result.Should().BeNull();
    }
}
