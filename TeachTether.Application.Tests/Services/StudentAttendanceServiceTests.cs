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

public class StudentAttendanceServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IStudentAttendanceRepository> _mockAttendanceRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly StudentAttendanceService _sut;

    public StudentAttendanceServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.StudentAttendances).Returns(_mockAttendanceRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _sut = new StudentAttendanceService(_mockUnitOfWork.Object, _mockMapper.Object, _mockUserService.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldAddAttendanceAndPopulateTeacherName()
    {
        // Arrange
        var request = CreateValidRequest();
        var attendance = CreateValidAttendance();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockMapper.Setup(m => m.Map<StudentAttendance>(request)).Returns(attendance);
        _mockMapper.Setup(m => m.Map<StudentAttendanceResponse>(attendance)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(attendance.TeacherId)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);

        // Act
        var result = await _sut.CreateAsync(request, teacherId: 1, studentId: 5);

        // Assert
        result.Should().NotBeNull();
        result.TeacherName.Should().Be("John Doe");
        _mockAttendanceRepo.Verify(r => r.AddAsync(attendance), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenAttendanceExists_ShouldDeleteAndSave()
    {
        // Arrange
        var attendance = CreateValidAttendance();
        _mockAttendanceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(attendance);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _mockAttendanceRepo.Verify(r => r.Delete(attendance), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAttendanceDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockAttendanceRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentAttendance?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Attendance Record not found");
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenAttendanceExists_ShouldReturnResponseWithTeacherName()
    {
        // Arrange
        var attendance = CreateValidAttendance();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockAttendanceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(attendance);
        _mockMapper.Setup(m => m.Map<StudentAttendanceResponse>(attendance)).Returns(response);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(attendance.TeacherId)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.TeacherName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenAttendanceDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockAttendanceRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentAttendance?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student attendances record not found");
    }

    // --- GetAllByStudentAsync ---

    [Fact]
    public async Task GetAllByStudentAsync_WhenCalled_ShouldReturnResponsesWithTeacherNames()
    {
        // Arrange
        var attendance = CreateValidAttendance();
        var response = CreateValidResponse();
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();

        _mockAttendanceRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StudentAttendance, bool>>>()))
            .ReturnsAsync(new List<StudentAttendance> { attendance });
        _mockTeacherRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Teacher> { teacher });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<IEnumerable<StudentAttendanceResponse>>(It.IsAny<IEnumerable<StudentAttendance>>()))
            .Returns(new List<StudentAttendanceResponse> { response });

        // Act
        var result = await _sut.GetAllByStudentAsync(studentId: 5, subjectId: 3);

        // Assert
        result.Should().HaveCount(1);
        result.First().TeacherName.Should().Be("John Doe");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenAttendanceExists_ShouldUpdateAndSave()
    {
        // Arrange
        var attendance = CreateValidAttendance();
        var request = new UpdateStudentAttendanceRequest { Status = "Present" };

        _mockAttendanceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(attendance);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockAttendanceRepo.Verify(r => r.Update(attendance), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenAttendanceDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockAttendanceRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StudentAttendance?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateStudentAttendanceRequest
        {
            Status = "Absent"
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student attendances record not found");
    }

    // --- Factory methods ---

    private static StudentAttendance CreateValidAttendance() => new()
    {
        Id = 1,
        StudentId = 5,
        TeacherId = 1,
        SubjectId = 3,
        AttendanceDate = new DateOnly(2026, 4, 1),
        Status = AttendanceStatus.Present,
        CreatedAt = DateTime.UtcNow
    };

    private static StudentAttendanceResponse CreateValidResponse() => new()
    {
        Id = 1,
        StudentId = 5,
        TeacherId = 1,
        SubjectId = 3,
        AttendanceDate = new DateOnly(2026, 4, 1),
        Status = "Present",
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

    private static CreateStudentAttendanceRequest CreateValidRequest() => new()
    {
        SubjectId = 3,
        AttendanceDate = new DateOnly(2026, 4, 1),
        Status = "Present"
    };
}
