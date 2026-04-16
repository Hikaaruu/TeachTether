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

public class ClassGroupSubjectServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IClassGroupsSubjectDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IClassGroupSubjectRepository> _mockCgsRepo = new();
    private readonly ClassGroupSubjectService _sut;

    public ClassGroupSubjectServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.ClassGroupsSubjects).Returns(_mockCgsRepo.Object);
        _sut = new ClassGroupSubjectService(_mockUnitOfWork.Object, _mockMapper.Object, _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenSubjectNotYetAssignedToClassGroup_ShouldAddAndSave()
    {
        // Arrange
        var request = new CreateClassGroupSubjectRequest { SubjectId = 3 };
        var entity = new ClassGroupSubject { SubjectId = 3, ClassGroupId = 10 };

        _mockCgsRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupSubject, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<ClassGroupSubject>(request)).Returns(entity);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(request, classGroupId: 10);

        // Assert
        _mockCgsRepo.Verify(r => r.AddAsync(entity), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSubjectAlreadyAssignedToClassGroup_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new CreateClassGroupSubjectRequest { SubjectId = 3 };
        _mockCgsRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupSubject, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, classGroupId: 10);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenSubjectIsAssignedToClassGroup_ShouldDelegateToHelper()
    {
        // Arrange
        var cgsEntry = new ClassGroupSubject { Id = 7, ClassGroupId = 10, SubjectId = 3 };
        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject> { cgsEntry });
        _mockDeletionHelper.Setup(h => h.DeleteClassGroupsSubjectAsync(7)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(classGroupId: 10, subjectId: 3);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteClassGroupsSubjectAsync(7), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSubjectIsNotAssignedToClassGroup_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10))
            .ReturnsAsync(new List<ClassGroupSubject>());

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(classGroupId: 10, subjectId: 99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Subject is not assigned to this class group");
    }
}
