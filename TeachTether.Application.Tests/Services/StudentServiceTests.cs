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

public class StudentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IStudentDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IStudentRepository> _mockStudentRepo = new();
    private readonly Mock<IClassGroupStudentRepository> _mockCgsRepo = new();
    private readonly Mock<IGuardianStudentRepository> _mockGuardianStudentRepo = new();
    private readonly StudentService _sut;

    public StudentServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockCgsRepo.Object);
        _mockUnitOfWork.Setup(u => u.GuardianStudents).Returns(_mockGuardianStudentRepo.Object);
        _sut = new StudentService(_mockUnitOfWork.Object, _mockUserService.Object, _mockMapper.Object,
            _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldCreateUserAndStudentAndReturnResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockUserService.Setup(s => s.CreateAsync(request.User, UserType.Student))
            .ReturnsAsync((user, "P@ssw0rd"));
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(user.UserName);
        result.Password.Should().Be("P@ssw0rd");
        _mockStudentRepo.Verify(r => r.AddAsync(It.Is<Student>(s =>
            s.UserId == user.Id && s.SchoolId == 1)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToStudentDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteStudentAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteStudentAsync(5), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenStudentExists_ShouldReturnStudentResponse()
    {
        // Arrange
        var student = CreateValidStudent();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockStudentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);
        _mockUserService.Setup(s => s.GetByIdAsync(student.UserId)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(student.Id);
        result.SchoolId.Should().Be(student.SchoolId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenStudentDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockStudentRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student not found");
    }

    // --- GetAllByClassGroupAsync ---

    [Fact]
    public async Task GetAllByClassGroupAsync_WhenCalled_ShouldReturnStudentsInClassGroup()
    {
        // Arrange
        var student = CreateValidStudent();
        var cgsEntry = new ClassGroupStudent { StudentId = student.Id, ClassGroupId = 10 };
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupStudent> { cgsEntry });
        _mockStudentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Student> { student });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllByClassGroupAsync(10);

        // Assert
        result.Should().HaveCount(1);
    }

    // --- GetAllBySchoolAsync ---

    [Fact]
    public async Task GetAllBySchoolAsync_WhenCalled_ShouldReturnAllStudentsInSchool()
    {
        // Arrange
        var student = CreateValidStudent();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockStudentRepo.Setup(r => r.GetBySchoolIdAsync(1)).ReturnsAsync(new List<Student> { student });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllBySchoolAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    // --- GetWithoutClassGroupAsync ---

    [Fact]
    public async Task GetWithoutClassGroupAsync_WhenCalled_ShouldReturnOnlyUnassignedStudents()
    {
        // Arrange
        var assignedStudent = CreateValidStudent(id: 1);
        var unassignedStudent = CreateValidStudent(id: 2, userId: "user-002");
        var user2 = CreateValidUser(id: "user-002");
        var userDto = CreateValidUserDto();

        _mockStudentRepo.Setup(r => r.GetBySchoolIdAsync(1))
            .ReturnsAsync(new List<Student> { assignedStudent, unassignedStudent });
        _mockCgsRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupStudent, bool>>>()))
            .ReturnsAsync(new List<ClassGroupStudent>
            {
                new() { StudentId = 1, ClassGroupId = 10 }
            });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user2 });
        _mockMapper.Setup(m => m.Map<UserDto>(user2)).Returns(userDto);

        // Act
        var result = await _sut.GetWithoutClassGroupAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
    }

    // --- Factory methods ---

    private static Student CreateValidStudent(int id = 1, string userId = "user-001") => new()
    {
        Id = id,
        UserId = userId,
        SchoolId = 1,
        DateOfBirth = new DateOnly(2010, 3, 15)
    };

    private static User CreateValidUser(string id = "user-001") => new()
    {
        Id = id,
        UserName = "student.one",
        FirstName = "Alice",
        LastName = "Smith",
        UserType = UserType.Student
    };

    private static UserDto CreateValidUserDto() => new()
    {
        FirstName = "Alice",
        LastName = "Smith",
        Sex = 'F'
    };

    private static CreateStudentRequest CreateValidRequest() => new()
    {
        User = new CreateUserDto { FirstName = "Alice", LastName = "Smith", Sex = 'F' },
        DateOfBirth = new DateOnly(2010, 3, 15)
    };
}
