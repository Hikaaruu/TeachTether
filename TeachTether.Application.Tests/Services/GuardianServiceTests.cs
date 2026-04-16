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

public class GuardianServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IGuardianDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IGuardianRepository> _mockGuardianRepo = new();
    private readonly Mock<IGuardianStudentRepository> _mockGuardianStudentRepo = new();
    private readonly Mock<IClassGroupStudentRepository> _mockCgsRepo = new();
    private readonly Mock<IClassGroupRepository> _mockClassGroupRepo = new();
    private readonly Mock<IClassAssignmentRepository> _mockAssignmentRepo = new();
    private readonly Mock<IClassGroupSubjectRepository> _mockCgSubjectRepo = new();
    private readonly GuardianService _sut;

    public GuardianServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Guardians).Returns(_mockGuardianRepo.Object);
        _mockUnitOfWork.Setup(u => u.GuardianStudents).Returns(_mockGuardianStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockCgsRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroups).Returns(_mockClassGroupRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassAssignments).Returns(_mockAssignmentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupsSubjects).Returns(_mockCgSubjectRepo.Object);
        _sut = new GuardianService(_mockUnitOfWork.Object, _mockUserService.Object, _mockMapper.Object,
            _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldCreateUserAndGuardianAndReturnResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockUserService.Setup(s => s.CreateAsync(request.User, UserType.Guardian))
            .ReturnsAsync((user, "P@ssw0rd"));
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(user.UserName);
        result.Password.Should().Be("P@ssw0rd");
        _mockGuardianRepo.Verify(r => r.AddAsync(It.Is<Guardian>(g =>
            g.UserId == user.Id && g.SchoolId == 1)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToGuardianDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteGuardianAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteGuardianAsync(5), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenGuardianExists_ShouldReturnGuardianResponse()
    {
        // Arrange
        var guardian = CreateValidGuardian();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockGuardianRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(guardian);
        _mockUserService.Setup(s => s.GetByIdAsync(guardian.UserId)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(guardian.Id);
        result.SchoolId.Should().Be(guardian.SchoolId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGuardianDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGuardianRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Guardian?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Guardian not found");
    }

    // --- GetAllBySchoolAsync ---

    [Fact]
    public async Task GetAllBySchoolAsync_WhenCalled_ShouldReturnAllGuardiansWithUserData()
    {
        // Arrange
        var guardian = CreateValidGuardian();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockGuardianRepo.Setup(r => r.GetBySchoolIdAsync(1)).ReturnsAsync(new List<Guardian> { guardian });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllBySchoolAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(guardian.Id);
    }

    // --- GetAllByStudentAsync ---

    [Fact]
    public async Task GetAllByStudentAsync_WhenCalled_ShouldReturnGuardiansForStudent()
    {
        // Arrange
        var guardian = CreateValidGuardian();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockGuardianStudentRepo.Setup(r => r.GetByStudentIdAsync(5))
            .ReturnsAsync(new List<GuardianStudent> { new() { StudentId = 5, GuardianId = 1 } });
        _mockGuardianRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Guardian> { guardian });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAllByStudentAsync(5);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(guardian.Id);
    }

    // --- GetAvailableForTeacherAsync ---

    [Fact]
    public async Task GetAvailableForTeacherAsync_WhenCalled_ShouldReturnGuardiansOfStudentsInTeacherClasses()
    {
        // Arrange
        var guardian = CreateValidGuardian();
        var user = CreateValidUser();
        var userDto = CreateValidUserDto();

        _mockClassGroupRepo.Setup(r => r.GetByHomeroomTeacherIdAsync(1))
            .ReturnsAsync(new List<ClassGroup> { new() { Id = 10, HomeroomTeacherId = 1 } });
        _mockAssignmentRepo.Setup(r => r.GetByTeacherIdAsync(1))
            .ReturnsAsync(new List<ClassAssignment>());
        _mockCgSubjectRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ClassGroupSubject>());
        _mockCgsRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupStudent, bool>>>()))
            .ReturnsAsync(new List<ClassGroupStudent> { new() { StudentId = 5, ClassGroupId = 10 } });
        _mockGuardianStudentRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GuardianStudent, bool>>>()))
            .ReturnsAsync(new List<GuardianStudent> { new() { GuardianId = 1, StudentId = 5 } });
        _mockGuardianRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Guardian> { guardian });
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<User> { user });
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetAvailableForTeacherAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(guardian.Id);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenGuardianExists_ShouldUpdateAndSave()
    {
        // Arrange
        var guardian = CreateValidGuardian();
        var request = new UpdateGuardianRequest
        {
            User = new UpdateUserDto { FirstName = "Jane", LastName = "Guardian", Sex = 'F' },
            DateOfBirth = new DateOnly(1985, 7, 20)
        };

        _mockGuardianRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(guardian);
        _mockUserService.Setup(s => s.UpdateAsync(guardian.UserId, request.User)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockUserService.Verify(s => s.UpdateAsync(guardian.UserId, request.User), Times.Once);
        _mockGuardianRepo.Verify(r => r.Update(guardian), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenGuardianDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGuardianRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Guardian?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateGuardianRequest
        {
            User = new UpdateUserDto { FirstName = "X", LastName = "Y", Sex = 'M' },
            DateOfBirth = DateOnly.MinValue
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Guardian not found");
    }

    // --- Factory methods ---

    private static Guardian CreateValidGuardian() => new()
    {
        Id = 1,
        UserId = "user-001",
        SchoolId = 1,
        DateOfBirth = new DateOnly(1980, 1, 1)
    };

    private static User CreateValidUser() => new()
    {
        Id = "user-001",
        UserName = "guardian.one",
        FirstName = "Parent",
        LastName = "One",
        UserType = UserType.Guardian
    };

    private static UserDto CreateValidUserDto() => new()
    {
        FirstName = "Parent",
        LastName = "One",
        Sex = 'F'
    };

    private static CreateGuardianRequest CreateValidRequest() => new()
    {
        User = new CreateUserDto { FirstName = "Parent", LastName = "One", Sex = 'F' },
        DateOfBirth = new DateOnly(1980, 1, 1)
    };
}
