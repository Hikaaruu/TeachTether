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

public class AnnouncementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IAnnouncementDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IAnnouncementRepository> _mockAnnouncementRepo = new();
    private readonly Mock<IAnnouncementClassGroupRepository> _mockAnnouncementCgRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly Mock<IGuardianRepository> _mockGuardianRepo = new();
    private readonly Mock<IGuardianStudentRepository> _mockGuardianStudentRepo = new();
    private readonly Mock<IClassGroupStudentRepository> _mockCgsStudentRepo = new();
    private readonly Mock<IStudentRepository> _mockStudentRepo = new();
    private readonly AnnouncementService _sut;

    public AnnouncementServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Announcements).Returns(_mockAnnouncementRepo.Object);
        _mockUnitOfWork.Setup(u => u.AnnouncementClassGroups).Returns(_mockAnnouncementCgRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _mockUnitOfWork.Setup(u => u.Guardians).Returns(_mockGuardianRepo.Object);
        _mockUnitOfWork.Setup(u => u.GuardianStudents).Returns(_mockGuardianStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockCgsStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
        _sut = new AnnouncementService(_mockUnitOfWork.Object, _mockMapper.Object, _mockUserService.Object, _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldAddAnnouncementAndClassGroupsAndSaveTwice()
    {
        // Arrange
        var request = new CreateAnnouncementRequest
        {
            Title = "Test",
            Message = "Hello",
            TargetAudience = "Student",
            ClassGroupIds = [1, 2]
        };
        var announcement = CreateValidAnnouncement();
        var response = new AnnouncementResponse { Id = 1, TeacherId = 1, Title = "Test", Message = "Hello" };

        _mockMapper.Setup(m => m.Map<Announcement>(request)).Returns(announcement);
        _mockMapper.Setup(m => m.Map<AnnouncementResponse>(announcement)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, teacherId: 1);

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockAnnouncementRepo.Verify(r => r.AddAsync(announcement), Times.Once);
        _mockAnnouncementCgRepo.Verify(r => r.AddAsync(It.IsAny<AnnouncementClassGroup>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToAnnouncementDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteAnnouncementAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteAnnouncementAsync(5), Times.Once);
    }

    // --- GetAllBySchoolId ---

    [Fact]
    public async Task GetAllBySchoolId_WhenCalled_ShouldReturnAnnouncementsForTeachersInSchool()
    {
        // Arrange
        var announcement = CreateValidAnnouncement();
        var teacher = new Teacher { Id = 1, UserId = "u1", SchoolId = 1, DateOfBirth = default };
        var responses = new List<AnnouncementResponse> { new() { Id = 1, Title = "Test", Message = "Hi" } };

        _mockAnnouncementRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Announcement> { announcement });
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(announcement.TeacherId)).ReturnsAsync(teacher);
        _mockMapper.Setup(m => m.Map<IEnumerable<AnnouncementResponse>>(It.IsAny<IEnumerable<Announcement>>()))
            .Returns(responses);

        // Act
        var result = await _sut.GetAllBySchoolId(1);

        // Assert
        result.Should().HaveCount(1);
    }

    // --- GetAllForUserAsync: Teacher branch ---

    [Fact]
    public async Task GetAllForUserAsync_WhenUserIsTeacher_ShouldReturnTeacherAnnouncements()
    {
        // Arrange
        var user = new User { Id = "u1", UserName = "ann.smith", FirstName = "Ann", LastName = "Smith", UserType = UserType.Teacher };
        var teacher = new Teacher { Id = 1, UserId = "u1", SchoolId = 1, DateOfBirth = default };
        var announcement = CreateValidAnnouncement();

        _mockUserService.Setup(s => s.GetByIdAsync("u1")).ReturnsAsync(user);
        _mockTeacherRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(teacher);
        _mockAnnouncementRepo.Setup(r => r.GetByTeacherIdAsync(1)).ReturnsAsync(new List<Announcement> { announcement });
        _mockMapper.Setup(m => m.Map<IEnumerable<AnnouncementResponse>>(It.IsAny<IEnumerable<Announcement>>()))
            .Returns(new List<AnnouncementResponse> { new() { Id = 1, Title = "Test", Message = "Hi" } });

        // Act
        var result = await _sut.GetAllForUserAsync("u1");

        // Assert
        result.Should().HaveCount(1);
    }

    // --- GetAllForUserAsync: Guardian branch ---

    [Fact]
    public async Task GetAllForUserAsync_WhenUserIsGuardian_ShouldReturnGuardianAnnouncements()
    {
        // Arrange
        var user = new User { Id = "u2", UserName = "bob.jones", FirstName = "Bob", LastName = "Jones", UserType = UserType.Guardian };
        var guardian = new Guardian { Id = 10, UserId = "u2", SchoolId = 1, DateOfBirth = default };
        var guardianStudent = new GuardianStudent { GuardianId = 10, StudentId = 5 };
        var cgsEntry = new ClassGroupStudent { StudentId = 5, ClassGroupId = 2 };
        var acgEntry = new AnnouncementClassGroup { AnnouncementId = 1, ClassGroupId = 2 };
        var announcement = new Announcement
        {
            Id = 1,
            TeacherId = 1,
            Title = "Test",
            Message = "Hi",
            TargetAudience = AudienceType.Guardian,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserService.Setup(s => s.GetByIdAsync("u2")).ReturnsAsync(user);
        _mockGuardianRepo.Setup(r => r.GetByUserIdAsync("u2")).ReturnsAsync(guardian);
        _mockGuardianStudentRepo.Setup(r => r.GetByGuardianIdAsync(10))
            .ReturnsAsync(new List<GuardianStudent> { guardianStudent });
        _mockCgsStudentRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupStudent, bool>>>()))
            .ReturnsAsync(new List<ClassGroupStudent> { cgsEntry });
        _mockAnnouncementCgRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AnnouncementClassGroup, bool>>>()))
            .ReturnsAsync(new List<AnnouncementClassGroup> { acgEntry });
        _mockAnnouncementRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Announcement> { announcement });
        _mockMapper.Setup(m => m.Map<IEnumerable<AnnouncementResponse>>(It.IsAny<IEnumerable<Announcement>>()))
            .Returns(new List<AnnouncementResponse> { new() { Id = 1, Title = "Test", Message = "Hi" } });

        // Act
        var result = await _sut.GetAllForUserAsync("u2");

        // Assert
        result.Should().HaveCount(1);
    }

    // --- GetAllForUserAsync: Student branch ---

    [Fact]
    public async Task GetAllForUserAsync_WhenUserIsStudentInClassGroup_ShouldReturnStudentAnnouncements()
    {
        // Arrange
        var user = new User { Id = "u3", UserName = "sam.lee", FirstName = "Sam", LastName = "Lee", UserType = UserType.Student };
        var student = new Student { Id = 20, UserId = "u3", SchoolId = 1, DateOfBirth = default };
        var cgsEntry = new ClassGroupStudent { StudentId = 20, ClassGroupId = 3 };
        var acgEntry = new AnnouncementClassGroup { AnnouncementId = 2, ClassGroupId = 3 };
        var announcement = new Announcement
        {
            Id = 2,
            TeacherId = 1,
            Title = "Student News",
            Message = "...",
            TargetAudience = AudienceType.Student,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserService.Setup(s => s.GetByIdAsync("u3")).ReturnsAsync(user);
        _mockStudentRepo.Setup(r => r.GetByUserIdAsync("u3")).ReturnsAsync(student);
        _mockCgsStudentRepo.Setup(r => r.GetByStudentIdAsync(20)).ReturnsAsync(cgsEntry);
        _mockAnnouncementCgRepo.Setup(r => r.GetByClassGroupIdAsync(3))
            .ReturnsAsync(new List<AnnouncementClassGroup> { acgEntry });
        _mockAnnouncementRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<Announcement> { announcement });
        _mockMapper.Setup(m => m.Map<IEnumerable<AnnouncementResponse>>(It.IsAny<IEnumerable<Announcement>>()))
            .Returns(new List<AnnouncementResponse> { new() { Id = 2, Title = "Student News", Message = "..." } });

        // Act
        var result = await _sut.GetAllForUserAsync("u3");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenStudentNotInClassGroup_ShouldReturnEmptyList()
    {
        // Arrange
        var user = new User { Id = "u4", UserName = "tom.brown", FirstName = "Tom", LastName = "Brown", UserType = UserType.Student };
        var student = new Student { Id = 25, UserId = "u4", SchoolId = 1, DateOfBirth = default };

        _mockUserService.Setup(s => s.GetByIdAsync("u4")).ReturnsAsync(user);
        _mockStudentRepo.Setup(r => r.GetByUserIdAsync("u4")).ReturnsAsync(student);
        _mockCgsStudentRepo.Setup(r => r.GetByStudentIdAsync(25)).ReturnsAsync((ClassGroupStudent?)null);
        _mockMapper.Setup(m => m.Map<IEnumerable<AnnouncementResponse>>(It.IsAny<IEnumerable<Announcement>>()))
            .Returns(new List<AnnouncementResponse>());

        // Act
        var result = await _sut.GetAllForUserAsync("u4");

        // Assert
        result.Should().BeEmpty();
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

    // --- Factory methods ---

    private static Announcement CreateValidAnnouncement() => new()
    {
        Id = 1,
        TeacherId = 1,
        Title = "Test",
        Message = "Hello",
        TargetAudience = AudienceType.Student,
        CreatedAt = DateTime.UtcNow
    };
}
