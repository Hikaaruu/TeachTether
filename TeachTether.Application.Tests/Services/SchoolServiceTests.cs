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

public class SchoolServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ISchoolDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<ISchoolRepository> _mockSchoolRepo = new();
    private readonly Mock<ISchoolOwnerRepository> _mockSchoolOwnerRepo = new();
    private readonly SchoolService _sut;

    public SchoolServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Schools).Returns(_mockSchoolRepo.Object);
        _mockUnitOfWork.Setup(u => u.SchoolOwners).Returns(_mockSchoolOwnerRepo.Object);
        _sut = new SchoolService(_mockUnitOfWork.Object, _mockMapper.Object, _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenOwnerExistsAndNameIsUnique_ShouldAddAndReturnSchoolResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var school = CreateValidSchool();
        var response = new SchoolResponse { Id = 1, Name = request.Name };

        _mockSchoolOwnerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SchoolOwner { Id = 1, UserId = "u1" });
        _mockSchoolRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<School, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<School>(request)).Returns(school);
        _mockMapper.Setup(m => m.Map<SchoolResponse>(school)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, 1);

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockSchoolRepo.Verify(r => r.AddAsync(school), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenOwnerDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSchoolOwnerRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SchoolOwner?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(CreateValidRequest(), 99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("School owner not found");
    }

    [Fact]
    public async Task CreateAsync_WhenSchoolNameAlreadyExistsForOwner_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = CreateValidRequest();
        _mockSchoolOwnerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new SchoolOwner { Id = 1, UserId = "u1" });
        _mockSchoolRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<School, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request, 1);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenSchoolExists_ShouldReturnMappedSchoolResponse()
    {
        // Arrange
        var school = CreateValidSchool();
        var response = new SchoolResponse { Id = 1, Name = school.Name };

        _mockSchoolRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(school);
        _mockMapper.Setup(m => m.Map<SchoolResponse>(school)).Returns(response);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSchoolDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSchoolRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((School?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("School not found");
    }

    // --- GetAllByOwnerAsync ---

    [Fact]
    public async Task GetAllByOwnerAsync_WhenCalled_ShouldReturnMappedSchoolResponses()
    {
        // Arrange
        var schools = new List<School> { CreateValidSchool() };
        var responses = new List<SchoolResponse> { new() { Id = 1, Name = "School" } };

        _mockSchoolRepo.Setup(r => r.GetBySchoolOwnerIdAsync(1)).ReturnsAsync(schools);
        _mockMapper.Setup(m => m.Map<IEnumerable<SchoolResponse>>(schools)).Returns(responses);

        // Act
        var result = await _sut.GetAllByOwnerAsync(1);

        // Assert
        result.Should().BeEquivalentTo(responses);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToSchoolDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteSchoolAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteSchoolAsync(5), Times.Once);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenSchoolExistsAndNameIsUnique_ShouldUpdateAndSave()
    {
        // Arrange
        var school = CreateValidSchool();
        var request = new UpdateSchoolRequest { Name = "New Name" };

        _mockSchoolRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(school);
        _mockSchoolRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<School, bool>>>()))
            .ReturnsAsync(false);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockSchoolRepo.Verify(r => r.Update(school), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenSchoolDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSchoolRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((School?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateSchoolRequest { Name = "X" });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("School not found");
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateNameExistsForOwner_ShouldThrowBadRequestException()
    {
        // Arrange
        var school = CreateValidSchool();
        _mockSchoolRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(school);
        _mockSchoolRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<School, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(1, new UpdateSchoolRequest { Name = "Duplicate" });

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- Factory methods ---

    private static School CreateValidSchool() => new()
    {
        Id = 1,
        Name = "Springfield Elementary",
        SchoolOwnerId = 1
    };

    private static CreateSchoolRequest CreateValidRequest() => new()
    {
        Name = "Springfield Elementary"
    };
}
