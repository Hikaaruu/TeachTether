using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common;
using TeachTether.Application.Common.Interfaces;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<ICredentialsGenerator> _mockCredentialsGen = new();
    private readonly Mock<IUserRepository> _mockUserRepo = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _mockCredentialsGen.Object,
            _mockUserRepo.Object,
            _mockMapper.Object,
            _mockJwtGenerator.Object);
    }

    // --- RegisterAsync ---

    [Fact]
    public async Task RegisterAsync_WhenCalled_ShouldCreateUserWithSchoolOwnerTypeAndReturnResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            UserName = "john.doe",
            Email = "john@school.com",
            Password = "Pass123!",
            FirstName = "John",
            LastName = "Doe",
            Sex = 'M'
        };
        var user = new User { Id = "u1", UserName = "john.doe", FirstName = "John", LastName = "Doe" };
        var result = OperationResult.Success();

        _mockMapper.Setup(m => m.Map<User>(request)).Returns(user);
        _mockUserRepo.Setup(r => r.CreateAsync(user, request.Password)).ReturnsAsync(result);

        // Act
        var (operationResult, returnedUser) = await _sut.RegisterAsync(request);

        // Assert
        operationResult.Succeeded.Should().BeTrue();
        returnedUser.UserType.Should().Be(UserType.SchoolOwner);
        _mockUserRepo.Verify(r => r.CreateAsync(user, request.Password), Times.Once);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenUsernameIsUnique_ShouldReturnUserAndPassword()
    {
        // Arrange
        var dto = new CreateUserDto { FirstName = "Jane", LastName = "Smith", Sex = 'F' };
        var user = new User { FirstName = "Jane", LastName = "Smith", UserName = "tbd" };
        const string generatedUsername = "jane.smith";
        const string generatedPassword = "Abc123!@#";

        _mockMapper.Setup(m => m.Map<User>(dto)).Returns(user);
        _mockCredentialsGen.Setup(g => g.GenerateUsername(user.FirstName, user.MiddleName, user.LastName))
            .Returns(generatedUsername);
        _mockUserRepo.Setup(r => r.FindByUserNameAsync(generatedUsername)).ReturnsAsync((User?)null);
        _mockCredentialsGen.Setup(g => g.GeneratePassword()).Returns(generatedPassword);
        _mockUserRepo.Setup(r => r.CreateAsync(user, generatedPassword)).ReturnsAsync(OperationResult.Success());

        // Act
        var (returnedUser, returnedPassword) = await _sut.CreateAsync(dto, UserType.Teacher);

        // Assert
        returnedUser.UserType.Should().Be(UserType.Teacher);
        returnedUser.UserName.Should().Be(generatedUsername);
        returnedPassword.Should().Be(generatedPassword);
        _mockUserRepo.Verify(r => r.CreateAsync(user, generatedPassword), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenFirstUsernameIsTaken_ShouldRetryUntilUniqueAndCreateUser()
    {
        // Arrange
        var dto = new CreateUserDto { FirstName = "Mark", LastName = "Taylor", Sex = 'M' };
        var user = new User { FirstName = "Mark", LastName = "Taylor", UserName = "tbd" };
        const string username = "mark.taylor";

        _mockMapper.Setup(m => m.Map<User>(dto)).Returns(user);
        _mockCredentialsGen.Setup(g => g.GenerateUsername(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(username);

        // First call returns existing user (taken), second returns null (free)
        var existing = new User { UserName = username, FirstName = "Mark", LastName = "Taylor" };
        _mockUserRepo.SetupSequence(r => r.FindByUserNameAsync(username))
            .ReturnsAsync(existing)
            .ReturnsAsync((User?)null);

        _mockCredentialsGen.Setup(g => g.GeneratePassword()).Returns("Pass999!");
        _mockUserRepo.Setup(r => r.CreateAsync(user, "Pass999!")).ReturnsAsync(OperationResult.Success());

        // Act
        var (returnedUser, _) = await _sut.CreateAsync(dto, UserType.Teacher);

        // Assert
        _mockUserRepo.Verify(r => r.FindByUserNameAsync(username), Times.Exactly(2));
        returnedUser.Should().NotBeNull();
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldMapAndUpdateUser()
    {
        // Arrange
        var user = new User { Id = "u1", UserName = "john.doe", FirstName = "John", LastName = "Doe" };
        var updateDto = new UpdateUserDto { FirstName = "Johnny", LastName = "Doe", Sex = 'M' };

        _mockUserRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.UpdateAsync(user)).ReturnsAsync(OperationResult.Success());

        // Act
        await _sut.UpdateAsync("u1", updateDto);

        // Assert
        _mockMapper.Verify(m => m.Map(updateDto, user), Times.Once);
        _mockUserRepo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    // --- TryLoginAsync ---

    [Fact]
    public async Task TryLoginAsync_WhenCredentialsAreValid_ShouldReturnJwtToken()
    {
        // Arrange
        var request = new LoginRequest { UserName = "john.doe", Password = "Pass123!" };
        var user = new User { Id = "u1", UserName = "john.doe", FirstName = "John", LastName = "Doe" };

        _mockUserRepo.Setup(r => r.FindByUserNameAsync("john.doe")).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.CheckPasswordAsync("u1", "Pass123!")).ReturnsAsync(true);
        _mockJwtGenerator.Setup(j => j.GenerateJwtTokenAsync(user)).ReturnsAsync("jwt-token-abc");

        // Act
        var result = await _sut.TryLoginAsync(request);

        // Assert
        result.Should().Be("jwt-token-abc");
    }

    [Fact]
    public async Task TryLoginAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var request = new LoginRequest { UserName = "unknown", Password = "Pass123!" };
        _mockUserRepo.Setup(r => r.FindByUserNameAsync("unknown")).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.TryLoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryLoginAsync_WhenPasswordIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var request = new LoginRequest { UserName = "john.doe", Password = "WrongPass" };
        var user = new User { Id = "u1", UserName = "john.doe", FirstName = "John", LastName = "Doe" };

        _mockUserRepo.Setup(r => r.FindByUserNameAsync("john.doe")).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.CheckPasswordAsync("u1", "WrongPass")).ReturnsAsync(false);

        // Act
        var result = await _sut.TryLoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Id = "u1", UserName = "john.doe", FirstName = "John", LastName = "Doe" };
        _mockUserRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync("u1");

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    // --- GetByIdsAsync ---

    [Fact]
    public async Task GetByIdsAsync_WhenCalled_ShouldReturnDistinctUsersFromRepository()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = "u1", UserName = "a.b", FirstName = "A", LastName = "B" },
            new() { Id = "u2", UserName = "c.d", FirstName = "C", LastName = "D" }
        };
        _mockUserRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(users);

        // Act
        var result = await _sut.GetByIdsAsync(new[] { "u1", "u2", "u1" });

        // Assert
        result.Should().HaveCount(2);
    }
}
