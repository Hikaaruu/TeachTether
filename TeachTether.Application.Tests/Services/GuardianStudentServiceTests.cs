using FluentAssertions;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class GuardianStudentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IGuardianStudentRepository> _mockGuardianStudentRepo = new();
    private readonly GuardianStudentService _sut;

    public GuardianStudentServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.GuardianStudents).Returns(_mockGuardianStudentRepo.Object);
        _sut = new GuardianStudentService(_mockUnitOfWork.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRelationshipDoesNotExist_ShouldAddAndSave()
    {
        // Arrange
        _mockGuardianStudentRepo
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GuardianStudent, bool>>>()))
            .ReturnsAsync(false);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(studentId: 1, guardianId: 2);

        // Assert
        _mockGuardianStudentRepo.Verify(r => r.AddAsync(It.Is<GuardianStudent>(gs =>
            gs.StudentId == 1 && gs.GuardianId == 2)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenStudentIsAlreadyAssignedToGuardian_ShouldThrowBadRequestException()
    {
        // Arrange
        _mockGuardianStudentRepo
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GuardianStudent, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(studentId: 1, guardianId: 2);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenRelationshipExists_ShouldDeleteAndSave()
    {
        // Arrange
        var guardianStudent = new GuardianStudent { StudentId = 1, GuardianId = 2 };
        _mockGuardianStudentRepo.Setup(r => r.GetByStudentIdAsync(1))
            .ReturnsAsync(new List<GuardianStudent> { guardianStudent });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(studentId: 1, guardianId: 2);

        // Assert
        _mockGuardianStudentRepo.Verify(r => r.Delete(guardianStudent), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRelationshipDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockGuardianStudentRepo.Setup(r => r.GetByStudentIdAsync(1))
            .ReturnsAsync(new List<GuardianStudent>());

        // Act
        Func<Task> act = async () => await _sut.DeleteAsync(studentId: 1, guardianId: 99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Student is not assigned to this guardian");
    }
}
