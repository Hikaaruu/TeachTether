using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class StudentBehaviorServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IStudentBehaviorRepository> _mockBehaviorRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly StudentBehaviorService _sut;

    public StudentBehaviorServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.StudentBehaviors).Returns(_mockBehaviorRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _sut = new StudentBehaviorService(_mockUnitOfWork.Object, _mockMapper.Object, _mockUserService.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldAddBehaviorAndPopulateTeacherName()
    {
        // Arrange
        var request = CreateValidRequest();
        var behavior = CreateValidBehavior();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockMapper.Setup(m => m.Map<StudentBehavior>(request)).Returns(behavior);
        _mockMapper.Setup(m => m.Map<StudentBehaviorResponse>(behavior)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(behavior.TeacherId)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);

        // Act
        var result = await _sut.CreateAsync(request, teacherId: 1, studentId: 5);

        // Assert
        result.Should().NotBeNull();
        result.TeacherName.Should().Be("John Doe");
        _mockBehaviorRepo.Verify(r => r.AddAsync(behavior), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenBehaviorExists_ShouldDeleteAndSave()
    {
        // Arrange
        var behavior = CreateValidBehavior();
        _mockBehaviorRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(behavior);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _mockBehaviorRepo.Verify(r => r.Delete(behavior), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenBehaviorDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockBehaviorRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentBehavior?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Behavior Record not found");
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenBehaviorExists_ShouldReturnResponseWithTeacherName()
    {
        // Arrange
        var behavior = CreateValidBehavior();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockBehaviorRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(behavior);
        _mockMapper.Setup(m => m.Map<StudentBehaviorResponse>(behavior)).Returns(response);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(behavior.TeacherId)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.TeacherName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenBehaviorDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockBehaviorRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentBehavior?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student behavior record not found");
    }

    // --- GetAllByStudentAsync ---

    [Fact]
    public async Task GetAllByStudentAsync_WhenCalled_ShouldReturnResponsesWithTeacherNames()
    {
        // Arrange
        var behavior = CreateValidBehavior();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockBehaviorRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentBehavior, bool>>>()))
            .ReturnsAsync(new List<StudentBehavior> { behavior });
        _mockTeacherRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Teacher> { teacher });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<IEnumerable<StudentBehaviorResponse>>(It.IsAny<IEnumerable<StudentBehavior>>()))
            .Returns(new List<StudentBehaviorResponse> { response });

        // Act
        var result = await _sut.GetAllByStudentAsync(studentId: 5, subjectId: 3);

        // Assert
        result.Should().HaveCount(1);
        result.First().TeacherName.Should().Be("John Doe");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenBehaviorExists_ShouldUpdateAndSave()
    {
        // Arrange
        var behavior = CreateValidBehavior();
        var request = new UpdateStudentBehaviorRequest { BehaviorScore = 9.0m };

        _mockBehaviorRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(behavior);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockBehaviorRepo.Verify(r => r.Update(behavior), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenBehaviorDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockBehaviorRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentBehavior?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateStudentBehaviorRequest
        {
            BehaviorScore = 7.0m
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student behavior record not found");
    }

    // --- Factory methods ---

    private static StudentBehavior CreateValidBehavior() => new()
    {
        Id = 1,
        StudentId = 5,
        TeacherId = 1,
        SubjectId = 3,
        BehaviorScore = 8.5m,
        BehaviorDate = new DateOnly(2026, 4, 1),
        CreatedAt = DateTime.UtcNow
    };

    private static StudentBehaviorResponse CreateValidResponse() => new()
    {
        Id = 1,
        StudentId = 5,
        TeacherId = 1,
        SubjectId = 3,
        BehaviorScore = 8.5m,
        BehaviorDate = new DateOnly(2026, 4, 1),
        TeacherName = "John Doe"
    };

    private static Teacher CreateValidTeacher() => new()
    {
        Id = 1,
        UserId = "user-001",
        SchoolId = 1,
        DateOfBirth = new DateOnly(1985, 5, 20)
    };

    private static User CreateValidUser() => new()
    {
        Id = "user-001",
        UserName = "john.doe",
        FirstName = "John",
        LastName = "Doe",
        UserType = UserType.Teacher
    };

    private static CreateStudentBehaviorRequest CreateValidRequest() => new()
    {
        SubjectId = 3,
        BehaviorScore = 8.5m,
        BehaviorDate = new DateOnly(2026, 4, 1)
    };
}
