using FluentAssertions;
using Moq;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class OwnerServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<ISchoolOwnerRepository> _mockSchoolOwnerRepo = new();
    private readonly OwnerService _sut;

    public OwnerServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.SchoolOwners).Returns(_mockSchoolOwnerRepo.Object);
        _sut = new OwnerService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ExistsAsync_WhenOwnerExists_ShouldReturnTrue()
    {
        // Arrange
        _mockSchoolOwnerRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SchoolOwner, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenOwnerDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockSchoolOwnerRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SchoolOwner, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ExistsAsync(99);

        // Assert
        result.Should().BeFalse();
    }
}
