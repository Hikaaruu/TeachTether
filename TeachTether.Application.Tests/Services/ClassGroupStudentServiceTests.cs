using AutoMapper;
using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class ClassGroupStudentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IClassGroupStudentRepository> _mockCgsRepo = new();
    private readonly ClassGroupStudentService _sut;

    public ClassGroupStudentServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.ClassGroupStudents).Returns(_mockCgsRepo.Object);
        _sut = new ClassGroupStudentService(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenStudentNotInAnyClassGroup_ShouldAddAndSave()
    {
        // Arrange
        var request = new CreateClassGroupStudentRequest { StudentId = 1 };
        var entity = new ClassGroupStudent { StudentId = 1, ClassGroupId = 5 };

        _mockCgsRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupStudent, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<ClassGroupStudent>(request)).Returns(entity);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(request, classGroupId: 5);

        // Assert
        _mockCgsRepo.Verify(r => r.AddAsync(entity), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenStudentAlreadyInClassGroup_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new CreateClassGroupStudentRequest { StudentId = 1 };
        _mockCgsRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ClassGroupStudent, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, classGroupId: 5);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenStudentExistsInClassGroup_ShouldDeleteAndSave()
    {
        // Arrange
        var entity = new ClassGroupStudent { StudentId = 10, ClassGroupId = 5 };
        _mockCgsRepo.Setup(r => r.GetByStudentIdAsync(10)).ReturnsAsync(entity);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(classGroupId: 5, studentId: 10);

        // Assert
        _mockCgsRepo.Verify(r => r.Delete(entity), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenStudentNotInAnyClassGroup_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockCgsRepo.Setup(r => r.GetByStudentIdAsync(10)).ReturnsAsync((ClassGroupStudent?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(classGroupId: 5, studentId: 10);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student is not in this class group");
    }

    [Fact]
    public async Task DeleteAsync_WhenStudentBelongsToDifferentClassGroup_ShouldThrowException()
    {
        // Arrange
        var entity = new ClassGroupStudent { StudentId = 10, ClassGroupId = 99 };
        _mockCgsRepo.Setup(r => r.GetByStudentIdAsync(10)).ReturnsAsync(entity);

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(classGroupId: 5, studentId: 10);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
