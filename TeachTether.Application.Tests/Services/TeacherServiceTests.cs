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

public class TeacherServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ITeacherDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly Mock<IClassGroupSubjectRepository> _mockCgsRepo = new();
    private readonly Mock<IClassAssignmentRepository> _mockAssignmentRepo = new();
    private readonly Mock<IGuardianStudentRepository> _mockGuardianStudentRepo = new();
    private readonly Mock<IClassGroupStudentRepository> _mockClassGroupStudentRepo = new();
    private readonly Mock<IClassGroupRepository> _mockClassGroupRepo = new();
    private readonly TeacherService _sut;

    public TeacherServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupsSubjects).Returns(_mockCgsRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassAssignments).Returns(_mockAssignmentRepo.Object);
        _mockUnitOfWork.Setup(u => u.GuardianStudents).Returns(_mockGuardianStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockClassGroupStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroups).Returns(_mockClassGroupRepo.Object);
        _sut = new TeacherService(_mockUnitOfWork.Object, _mockUserService.Object, _mockMapper.Object,
            _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldCreateUserAndTeacherAndReturnResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockUserService.Setup(s => s.CreateAsync(request.User, UserType.Teacher))
            .ReturnsAsync((user, "P@ssw0rd"));
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(user.UserName);
        result.Password.Should().Be("P@ssw0rd");
        _mockTeacherRepo.Verify(r => r.AddAsync(It.Is<Teacher>(t =>
            t.UserId == user.Id && t.SchoolId == 1)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToTeacherDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteTeacherAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteTeacherAsync(5), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenTeacherExists_ShouldReturnTeacherResponse()
    {
        // Arrange
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockTeacherRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.GetByIdAsync(teacher.UserId)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(teacher.Id);
        result.SchoolId.Should().Be(teacher.SchoolId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTeacherDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Teacher?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Teacher not found");
    }

    // --- GetAllBySchoolAsync ---

    [Fact]
    public async Task GetAllBySchoolAsync_WhenCalled_ShouldReturnAllTeachersWithUserData()
    {
        // Arrange
        var teacher = CreateValidTeacher();
        var teachers = new List<Teacher> { teacher };
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockTeacherRepo.Setup(r => r.GetBySchoolIdAsync(1)).ReturnsAsync(teachers);
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllBySchoolAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(teacher.Id);
    }

    // --- GetAllByClassGroupSubjectAsync ---

    [Fact]
    public async Task GetAllByClassGroupSubjectAsync_WhenSubjectIsAssignedToClassGroup_ShouldReturnTeachers()
    {
        // Arrange
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };
        var assignment = new ClassAssignment { TeacherId = 1, ClassGroupSubjectId = 7 };
        var teacher = CreateValidTeacher();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject> { cgsEntry });
        _mockAssignmentRepo.Setup(r => r.GetByClassGroupSubjectIdAsync(7))
            .ReturnsAsync(new List<ClassAssignment> { assignment });
        _mockTeacherRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Teacher> { teacher });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllByClassGroupSubjectAsync(classGroupId: 10, subjectId: 3);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllByClassGroupSubjectAsync_WhenSubjectNotAssignedToClassGroup_ShouldThrowBadRequestException()
    {
        // Arrange
        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject>());

        // Act
        Func<Task> act = async () => await _sut.GetAllByClassGroupSubjectAsync(classGroupId: 10, subjectId: 99);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenTeacherExists_ShouldUpdateAndSave()
    {
        // Arrange
        var teacher = CreateValidTeacher();
        var request = new UpdateTeacherRequest
        {
            User = new UpdateUserDto { FirstName = "Jane", LastName = "Smith", Sex = 'F' },
            DateOfBirth = new DateOnly(1990, 6, 15)
        };

        _mockTeacherRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(teacher);
        _mockUserService.Setup(s => s.UpdateAsync(teacher.UserId, request.User)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockUserService.Verify(s => s.UpdateAsync(teacher.UserId, request.User), Times.Once);
        _mockTeacherRepo.Verify(r => r.Update(teacher), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTeacherDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Teacher?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateTeacherRequest
        {
            User = new UpdateUserDto { FirstName = "X", LastName = "Y", Sex = 'M' },
            DateOfBirth = DateOnly.MinValue
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Teacher not found");
    }

    // --- Factory methods ---

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

    private static UserDto CreateValidUserDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Sex = 'M'
    };

    private static CreateTeacherRequest CreateValidRequest() => new()
    {
        User = new CreateUserDto { FirstName = "John", LastName = "Doe", Sex = 'M' },
        DateOfBirth = new DateOnly(1985, 5, 20)
    };
}
