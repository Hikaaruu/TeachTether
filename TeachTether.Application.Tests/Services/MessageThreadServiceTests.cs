using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class MessageThreadServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMessageThreadDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IMessageThreadRepository> _mockThreadRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly Mock<IGuardianRepository> _mockGuardianRepo = new();
    private readonly MessageThreadService _sut;

    public MessageThreadServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.MessageThreads).Returns(_mockThreadRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _mockUnitOfWork.Setup(u => u.Guardians).Returns(_mockGuardianRepo.Object);
        _sut = new MessageThreadService(_mockUnitOfWork.Object, _mockMapper.Object, _mockUserService.Object, _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenThreadDoesNotExist_ShouldAddThreadAndSave()
    {
        // Arrange
        var request = new CreateMessageThreadRequest { TeacherId = 1, GuardianId = 2 };
        var thread = new MessageThread { Id = 1, TeacherId = 1, GuardianId = 2 };
        var response = new MessageThreadResponse { Id = 1, TeacherId = 1, GuardianId = 2 };

        _mockThreadRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MessageThread, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<MessageThread>(request)).Returns(thread);
        _mockMapper.Setup(m => m.Map<MessageThreadResponse>(thread)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockThreadRepo.Verify(r => r.AddAsync(thread), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenThreadAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new CreateMessageThreadRequest { TeacherId = 1, GuardianId = 2 };
        _mockThreadRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MessageThread, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToMessageThreadDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteMessageThreadAsync(3)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(3);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteMessageThreadAsync(3), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenThreadExists_ShouldReturnMessageThreadResponse()
    {
        // Arrange
        var thread = new MessageThread { Id = 1, TeacherId = 1, GuardianId = 2 };
        var response = new MessageThreadResponse { Id = 1, TeacherId = 1, GuardianId = 2 };

        _mockThreadRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(thread);
        _mockMapper.Setup(m => m.Map<MessageThreadResponse>(thread)).Returns(response);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdAsync_WhenThreadDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockThreadRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MessageThread?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("thread not found");
    }

    // --- GetAllForUserAsync: Teacher branch ---

    [Fact]
    public async Task GetAllForUserAsync_WhenUserIsTeacher_ShouldReturnTeacherThreads()
    {
        // Arrange
        var user = new User { Id = "u1", UserName = "ann.smith", FirstName = "Ann", LastName = "Smith", UserType = UserType.Teacher };
        var teacher = new Teacher { Id = 1, UserId = "u1", SchoolId = 1, DateOfBirth = default };
        var thread = new MessageThread { Id = 1, TeacherId = 1, GuardianId = 2 };

        _mockUserService.Setup(s => s.GetByIdAsync("u1")).ReturnsAsync(user);
        _mockTeacherRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(teacher);
        _mockThreadRepo.Setup(r => r.GetByTeacherIdAsync(1)).ReturnsAsync(new List<MessageThread> { thread });
        _mockMapper.Setup(m => m.Map<IEnumerable<MessageThreadResponse>>(It.IsAny<IEnumerable<MessageThread>>()))
            .Returns(new List<MessageThreadResponse> { new() { Id = 1, TeacherId = 1, GuardianId = 2 } });

        // Act
        var result = await _sut.GetAllForUserAsync("u1");

        // Assert
        result.Should().HaveCount(1);
    }

    // --- GetAllForUserAsync: Guardian branch ---

    [Fact]
    public async Task GetAllForUserAsync_WhenUserIsGuardian_ShouldReturnGuardianThreads()
    {
        // Arrange
        var user = new User { Id = "u2", UserName = "bob.jones", FirstName = "Bob", LastName = "Jones", UserType = UserType.Guardian };
        var guardian = new Guardian { Id = 10, UserId = "u2", SchoolId = 1, DateOfBirth = default };
        var thread = new MessageThread { Id = 2, TeacherId = 1, GuardianId = 10 };

        _mockUserService.Setup(s => s.GetByIdAsync("u2")).ReturnsAsync(user);
        _mockGuardianRepo.Setup(r => r.GetByUserIdAsync("u2")).ReturnsAsync(guardian);
        _mockThreadRepo.Setup(r => r.GetByGuardianIdAsync(10)).ReturnsAsync(new List<MessageThread> { thread });
        _mockMapper.Setup(m => m.Map<IEnumerable<MessageThreadResponse>>(It.IsAny<IEnumerable<MessageThread>>()))
            .Returns(new List<MessageThreadResponse> { new() { Id = 2, TeacherId = 1, GuardianId = 10 } });

        // Act
        var result = await _sut.GetAllForUserAsync("u2");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetByIdAsync("missing")).ReturnsAsync((User)null!);

        // Act
        Func<Task> act = async () => await _sut.GetAllForUserAsync("missing");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found");
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenUserTypeIsUnexpected_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = "u5", UserName = "x.y", FirstName = "X", LastName = "Y", UserType = UserType.SchoolOwner };
        _mockUserService.Setup(s => s.GetByIdAsync("u5")).ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.GetAllForUserAsync("u5");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected behavior occurred");
    }
}
