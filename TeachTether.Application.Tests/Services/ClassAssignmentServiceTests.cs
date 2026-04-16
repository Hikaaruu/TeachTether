using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class ClassAssignmentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IClassGroupSubjectRepository> _mockCgSubjectRepo = new();
    private readonly Mock<IClassAssignmentRepository> _mockAssignmentRepo = new();
    private readonly Mock<IClassGroupRepository> _mockClassGroupRepo = new();
    private readonly Mock<ISubjectRepository> _mockSubjectRepo = new();
    private readonly ClassAssignmentService _sut;

    public ClassAssignmentServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.ClassGroupsSubjects).Returns(_mockCgSubjectRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassAssignments).Returns(_mockAssignmentRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroups).Returns(_mockClassGroupRepo.Object);
        _mockUnitOfWork.Setup(u => u.Subjects).Returns(_mockSubjectRepo.Object);
        _sut = new ClassAssignmentService(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenSubjectIsAssignedAndTeacherNotYetAssigned_ShouldAddAndSave()
    {
        // Arrange
        var request = new CreateClassAssignmentRequest { TeacherId = 1 };
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };
        var assignment = new ClassAssignment { TeacherId = 1, ClassGroupSubjectId = 7 };

        _mockCgSubjectRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject> { cgsEntry });
        _mockAssignmentRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassAssignment, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<ClassAssignment>(request)).Returns(assignment);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(request, classGroupId: 10, subjectId: 3);

        // Assert
        _mockAssignmentRepo.Verify(r => r.AddAsync(assignment), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSubjectNotAssignedToClassGroup_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockCgSubjectRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject>());

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(
            new CreateClassAssignmentRequest { TeacherId = 1 }, classGroupId: 10, subjectId: 99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("This subject is not assigned to specified class group");
    }

    [Fact]
    public async Task CreateAsync_WhenTeacherAlreadyAssignedToClassGroupSubject_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new CreateClassAssignmentRequest { TeacherId = 1 };
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };

        _mockCgSubjectRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject> { cgsEntry });
        _mockAssignmentRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassAssignment, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, classGroupId: 10, subjectId: 3);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenAssignmentExists_ShouldDeleteAndSave()
    {
        // Arrange
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };
        var assignment = new ClassAssignment { TeacherId = 1, ClassGroupSubjectId = 7 };

        _mockCgSubjectRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject> { cgsEntry });
        _mockAssignmentRepo.Setup(r => r.GetByClassGroupSubjectIdAsync(7))
            .ReturnsAsync(new List<ClassAssignment> { assignment });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(classGroupId: 10, subjectId: 3, teacherId: 1);

        // Assert
        _mockAssignmentRepo.Verify(r => r.Delete(assignment), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSubjectNotAssignedToClassGroup_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockCgSubjectRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject>());

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(classGroupId: 10, subjectId: 99, teacherId: 1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("This subject is not assigned to specified class group");
    }

    [Fact]
    public async Task DeleteAsync_WhenAssignmentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };

        _mockCgSubjectRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject> { cgsEntry });
        _mockAssignmentRepo.Setup(r => r.GetByClassGroupSubjectIdAsync(7))
            .ReturnsAsync(new List<ClassAssignment>());

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(classGroupId: 10, subjectId: 3, teacherId: 99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Class assignment not found");
    }

    // --- GetByTeacherAsync ---

    [Fact]
    public async Task GetByTeacherAsync_WhenCalled_ShouldReturnResponsesWithClassGroupAndSubjectNames()
    {
        // Arrange
        var assignment = new ClassAssignment { TeacherId = 1, ClassGroupSubjectId = 7 };
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };
        var classGroup = new ClassGroup { Id = 10, GradeYear = 9, Section = 'A', SchoolId = 1 };
        var subject = new Subject { Id = 3, Name = "Mathematics", SchoolId = 1 };

        _mockAssignmentRepo.Setup(r => r.GetByTeacherIdAsync(1))
            .ReturnsAsync(new List<ClassAssignment> { assignment });
        _mockCgSubjectRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(cgsEntry);
        _mockClassGroupRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(classGroup);
        _mockSubjectRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(subject);

        // Act
        var result = await _sut.GetByTeacherAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().ClassGroupName.Should().Be("9 - A");
        result.First().SubjectName.Should().Be("Mathematics");
    }
}
