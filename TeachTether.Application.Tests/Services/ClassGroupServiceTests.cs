using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class ClassGroupServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IClassGroupDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IClassGroupRepository> _mockClassGroupRepo = new();
    private readonly Mock<ITeacherRepository> _mockTeacherRepo = new();
    private readonly Mock<IClassGroupStudentRepository> _mockCgsStudentRepo = new();
    private readonly Mock<IClassAssignmentRepository> _mockAssignmentRepo = new();
    private readonly Mock<IClassGroupSubjectRepository> _mockCgSubjectRepo = new();
    private readonly ClassGroupService _sut;

    public ClassGroupServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.ClassGroups).Returns(_mockClassGroupRepo.Object);
        _mockUnitOfWork.Setup(u => u.Teachers).Returns(_mockTeacherRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockCgsStudentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassAssignments).Returns(_mockAssignmentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupsSubjects).Returns(_mockCgSubjectRepo.Object);
        _sut = new ClassGroupService(_mockUnitOfWork.Object, _mockMapper.Object, _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_ShouldAddAndReturnClassGroupResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var teacher = CreateValidTeacher(schoolId: 1);
        var classGroup = CreateValidClassGroup();
        var response = new ClassGroupResponse { Id = 1, GradeYear = 9, Section = 'A', SchoolId = 1 };

        _mockTeacherRepo.Setup(r => r.GetByIdAsync(request.HomeroomTeacherId)).ReturnsAsync(teacher);
        _mockClassGroupRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroup, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<ClassGroup>(request)).Returns(classGroup);
        _mockMapper.Setup(m => m.Map<ClassGroupResponse>(classGroup)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockClassGroupRepo.Verify(r => r.AddAsync(classGroup), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTeacherNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = CreateValidRequest();
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(request.HomeroomTeacherId)).ReturnsAsync((Teacher?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Teacher not found");
    }

    [Fact]
    public async Task CreateAsync_WhenTeacherBelongsToDifferentSchool_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = CreateValidRequest();
        var teacher = CreateValidTeacher(schoolId: 99); // different school
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(request.HomeroomTeacherId)).ReturnsAsync(teacher);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Teacher not found");
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateGradeAndSectionExistInSchool_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = CreateValidRequest();
        var teacher = CreateValidTeacher(schoolId: 1);
        _mockTeacherRepo.Setup(r => r.GetByIdAsync(request.HomeroomTeacherId)).ReturnsAsync(teacher);
        _mockClassGroupRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroup, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, schoolId: 1);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToClassGroupDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteClassGroupAsync(7)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(7);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteClassGroupAsync(7), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenClassGroupExists_ShouldReturnClassGroupResponse()
    {
        // Arrange
        var classGroup = CreateValidClassGroup();
        var response = new ClassGroupResponse { Id = 1, GradeYear = 9, Section = 'A', SchoolId = 1 };

        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(classGroup);
        _mockMapper.Setup(m => m.Map<ClassGroupResponse>(classGroup)).Returns(response);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdAsync_WhenClassGroupDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ClassGroup?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Class Group not found");
    }

    // --- GetAllBySchoolAsync ---

    [Fact]
    public async Task GetAllBySchoolAsync_WhenCalled_ShouldReturnMappedClassGroupResponses()
    {
        // Arrange
        var classGroups = new List<ClassGroup> { CreateValidClassGroup() };
        var responses = new List<ClassGroupResponse> { new() { Id = 1 } };

        _mockClassGroupRepo.Setup(r => r.GetBySchoolIdAsync(1)).ReturnsAsync(classGroups);
        _mockMapper.Setup(m => m.Map<IEnumerable<ClassGroupResponse>>(classGroups)).Returns(responses);

        // Act
        var result = await _sut.GetAllBySchoolAsync(1);

        // Assert
        result.Should().BeEquivalentTo(responses);
    }

    // --- GetAllByTeacherAsync ---

    [Fact]
    public async Task GetAllByTeacherAsync_WhenCalled_ShouldReturnClassGroupsForHomeroomTeacher()
    {
        // Arrange
        var classGroups = new List<ClassGroup> { CreateValidClassGroup() };
        var responses = new List<ClassGroupResponse> { new() { Id = 1 } };

        _mockClassGroupRepo.Setup(r => r.GetByHomeroomTeacherIdAsync(1)).ReturnsAsync(classGroups);
        _mockMapper.Setup(m => m.Map<IEnumerable<ClassGroupResponse>>(classGroups)).Returns(responses);

        // Act
        var result = await _sut.GetAllByTeacherAsync(1);

        // Assert
        result.Should().BeEquivalentTo(responses);
    }

    // --- GetAvailableForTeacherAsync ---

    [Fact]
    public async Task GetAvailableForTeacherAsync_WhenCalled_ShouldReturnUnionOfAssignedAndHomeroomClassGroups()
    {
        // Arrange
        var homeClassGroup = CreateValidClassGroup(id: 1);
        var assignedClassGroup = CreateValidClassGroup(id: 2);
        var classGroupSubject = new ClassGroupSubject { Id = 10, ClassGroupId = 2, SubjectId = 5 };
        var assignment = new ClassAssignment { TeacherId = 1, ClassGroupSubjectId = 10 };

        _mockAssignmentRepo.Setup(r => r.GetByTeacherIdAsync(1))
            .ReturnsAsync(new List<ClassAssignment> { assignment });
        _mockCgSubjectRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ClassGroupSubject> { classGroupSubject });
        _mockClassGroupRepo.Setup(r => r.GetByHomeroomTeacherIdAsync(1))
            .ReturnsAsync(new List<ClassGroup> { homeClassGroup });
        _mockClassGroupRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ClassGroup> { homeClassGroup, assignedClassGroup });
        _mockMapper.Setup(m => m.Map<IEnumerable<ClassGroupResponse>>(It.IsAny<IEnumerable<ClassGroup>>()))
            .Returns(new List<ClassGroupResponse> { new() { Id = 1 }, new() { Id = 2 } });

        // Act
        var result = await _sut.GetAvailableForTeacherAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    // --- GetByStudentAsync ---

    [Fact]
    public async Task GetByStudentAsync_WhenStudentIsInClassGroup_ShouldReturnClassGroupResponse()
    {
        // Arrange
        var cgsEntry = new ClassGroupStudent { StudentId = 5, ClassGroupId = 1 };
        var classGroup = CreateValidClassGroup();
        var response = new ClassGroupResponse { Id = 1, GradeYear = 9, Section = 'A', SchoolId = 1 };

        _mockCgsStudentRepo.Setup(r => r.GetByStudentIdAsync(5)).ReturnsAsync(cgsEntry);
        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(classGroup);
        _mockMapper.Setup(m => m.Map<ClassGroupResponse>(classGroup)).Returns(response);

        // Act
        var result = await _sut.GetByStudentAsync(5);

        // Assert
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByStudentAsync_WhenStudentNotInClassGroup_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockCgsStudentRepo.Setup(r => r.GetByStudentIdAsync(99)).ReturnsAsync((ClassGroupStudent?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByStudentAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student is not in class group");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenClassGroupExistsAndSectionGradeIsUnique_ShouldUpdateAndSave()
    {
        // Arrange
        var classGroup = CreateValidClassGroup();
        var request = new UpdateClassGroupRequest { GradeYear = 10, Section = 'B', HomeroomTeacherId = 1 };

        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(classGroup);
        _mockClassGroupRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroup, bool>>>()))
            .ReturnsAsync(false);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockClassGroupRepo.Verify(r => r.Update(classGroup), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenClassGroupDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ClassGroup?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateClassGroupRequest
        {
            GradeYear = 9,
            Section = 'A',
            HomeroomTeacherId = 1
        });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Class Group not found");
    }

    [Fact]
    public async Task UpdateAsync_WhenNewGradeAndSectionAlreadyExistInSchool_ShouldThrowBadRequestException()
    {
        // Arrange
        var classGroup = CreateValidClassGroup();
        // request uses different values to trigger the duplication check
        var request = new UpdateClassGroupRequest { GradeYear = 10, Section = 'B', HomeroomTeacherId = 1 };

        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(classGroup);
        _mockClassGroupRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroup, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- Factory methods ---

    private static ClassGroup CreateValidClassGroup(int id = 1) => new()
    {
        Id = id,
        GradeYear = 9,
        Section = 'A',
        HomeroomTeacherId = 1,
        SchoolId = 1
    };

    private static Teacher CreateValidTeacher(int schoolId = 1) => new()
    {
        Id = 1,
        UserId = "user-001",
        SchoolId = schoolId,
        DateOfBirth = new DateOnly(1985, 5, 20)
    };

    private static CreateClassGroupRequest CreateValidRequest() => new()
    {
        GradeYear = 9,
        Section = 'A',
        HomeroomTeacherId = 1
    };
}
