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

public class StudentGradeServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IStudentGradeRepository> _mockGradeRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly StudentGradeService _sut;

    public StudentGradeServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.StudentGrades).Returns(_mockGradeRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _sut = new StudentGradeService(_mockUnitOfWork.Object, _mockMapper.Object, _mockUserService.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldAddGradeAndPopulateTeacherName()
    {
        // Arrange
        var request = CreateValidRequest();
        var grade = CreateValidGrade();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockMapper.Setup(m => m.Map<StudentGrade>(request)).Returns(grade);
        _mockMapper.Setup(m => m.Map<StudentGradeResponse>(grade)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(grade.TeacherId)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);

        // Act
        var result = await _sut.CreateAsync(request, teacherId: 1, studentId: 5);

        // Assert
        result.Should().NotBeNull();
        result.TeacherName.Should().Be("John Doe");
        _mockGradeRepo.Verify(r => r.AddAsync(grade), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenGradeExists_ShouldDeleteAndSave()
    {
        // Arrange
        var grade = CreateValidGrade();
        _mockGradeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(grade);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _mockGradeRepo.Verify(r => r.Delete(grade), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenGradeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGradeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentGrade?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Grade Record not found");
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenGradeExists_ShouldReturnResponseWithTeacherName()
    {
        // Arrange
        var grade = CreateValidGrade();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockGradeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(grade);
        _mockMapper.Setup(m => m.Map<StudentGradeResponse>(grade)).Returns(response);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(grade.TeacherId)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.TeacherName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenGradeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGradeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentGrade?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student grade record not found");
    }

    // --- GetAllByStudentAsync ---

    [Fact]
    public async Task GetAllByStudentAsync_WhenCalled_ShouldReturnResponsesWithTeacherNames()
    {
        // Arrange
        var grade = CreateValidGrade();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockGradeRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentGrade, bool>>>()))
            .ReturnsAsync(new List<StudentGrade> { grade });
        _mockTeacherRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Teacher> { teacher });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<IEnumerable<StudentGradeResponse>>(It.IsAny<IEnumerable<StudentGrade>>()))
            .Returns(new List<StudentGradeResponse> { response });

        // Act
        var result = await _sut.GetAllByStudentAsync(studentId: 5, subjectId: 3);

        // Assert
        result.Should().HaveCount(1);
        result.First().TeacherName.Should().Be("John Doe");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenGradeExists_ShouldUpdateAndSave()
    {
        // Arrange
        var grade = CreateValidGrade();
        var request = new UpdateStudentGradeRequest { GradeValue = 9.5m, GradeType = "Exam" };

        _mockGradeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(grade);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockGradeRepo.Verify(r => r.Update(grade), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenGradeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGradeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentGrade?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateStudentGradeRequest
        {
            GradeValue = 8.0m,
            GradeType = "Quiz"
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student grade record not found");
    }

    // --- Factory methods ---

    private static StudentGrade CreateValidGrade() => new()
    {
        Id = 1,
        StudentId = 5,
        TeacherId = 1,
        SubjectId = 3,
        GradeValue = 8.5m,
        GradeDate = new DateOnly(2026, 4, 1),
        CreatedAt = DateTime.UtcNow
    };

    private static StudentGradeResponse CreateValidResponse() => new()
    {
        Id = 1,
        StudentId = 5,
        TeacherId = 1,
        SubjectId = 3,
        GradeValue = 8.5m,
        GradeType = "Exam",
        GradeDate = new DateOnly(2026, 4, 1),
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

    private static CreateStudentGradeRequest CreateValidRequest() => new()
    {
        SubjectId = 3,
        GradeValue = 8.5m,
        GradeType = "Exam",
        GradeDate = new DateOnly(2026, 4, 1)
    };
}
