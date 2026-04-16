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

public class SubjectServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ISubjectDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<ISubjectRepository> _mockSubjectRepo = new();
    private readonly Mock<IClassGroupSubjectRepository> _mockCgsRepo = new();
    private readonly SubjectService _sut;

    public SubjectServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Subjects).Returns(_mockSubjectRepo.Object);
        _mockUnitOfWork.Setup(u => u.ClassGroupsSubjects).Returns(_mockCgsRepo.Object);
        _sut = new SubjectService(_mockUnitOfWork.Object, _mockMapper.Object, _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenSubjectNameIsUniqueInSchool_ShouldAddAndReturnSubjectResponse()
    {
        // Arrange
        var request = CreateValidRequest();
        var subject = CreateValidSubject();
        var response = new SubjectResponse { Id = 1, Name = request.Name };

        _mockSubjectRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subject, bool>>>()))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<Subject>(request)).Returns(subject);
        _mockMapper.Setup(m => m.Map<SubjectResponse>(subject)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, 1);

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockSubjectRepo.Verify(r => r.AddAsync(subject), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSubjectNameAlreadyExistsInSchool_ShouldThrowBadRequestException()
    {
        // Arrange
        _mockSubjectRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subject, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(CreateValidRequest(), 1);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenSubjectExists_ShouldReturnMappedSubjectResponse()
    {
        // Arrange
        var subject = CreateValidSubject();
        var response = new SubjectResponse { Id = 1, Name = subject.Name };

        _mockSubjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _mockMapper.Setup(m => m.Map<SubjectResponse>(subject)).Returns(response);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSubjectDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSubjectRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Subject?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Subject not found");
    }

    // --- GetAllBySchoolAsync ---

    [Fact]
    public async Task GetAllBySchoolAsync_WhenCalled_ShouldReturnMappedSubjectResponses()
    {
        // Arrange
        var subjects = new List<Subject> { CreateValidSubject() };
        var responses = new List<SubjectResponse> { new() { Id = 1, Name = "Math" } };

        _mockSubjectRepo.Setup(r => r.GetBySchoolIdAsync(1)).ReturnsAsync(subjects);
        _mockMapper.Setup(m => m.Map<IEnumerable<SubjectResponse>>(subjects)).Returns(responses);

        // Act
        var result = await _sut.GetAllBySchoolAsync(1);

        // Assert
        result.Should().BeEquivalentTo(responses);
    }

    // --- GetAllByClassGroupAsync ---

    [Fact]
    public async Task GetAllByClassGroupAsync_WhenCalled_ShouldReturnSubjectsForClassGroup()
    {
        // Arrange
        var cgsEntries = new List<ClassGroupSubject> { new() { SubjectId = 1, ClassGroupId = 10 } };
        var subjects = new List<Subject> { CreateValidSubject() };
        var responses = new List<SubjectResponse> { new() { Id = 1, Name = "Math" } };

        _mockCgsRepo.Setup(r => r.GetByClassGroupIdAsync(10)).ReturnsAsync(cgsEntries);
        _mockSubjectRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(subjects);
        _mockMapper.Setup(m => m.Map<IEnumerable<SubjectResponse>>(subjects)).Returns(responses);

        // Act
        var result = await _sut.GetAllByClassGroupAsync(10);

        // Assert
        result.Should().BeEquivalentTo(responses);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToSubjectDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteSubjectAsync(3)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(3);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteSubjectAsync(3), Times.Once);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenSubjectExistsAndNameIsUnique_ShouldUpdateAndSave()
    {
        // Arrange
        var subject = CreateValidSubject();
        var request = new UpdateSubjectRequest { Name = "New Science" };

        _mockSubjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _mockSubjectRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subject, bool>>>()))
            .ReturnsAsync(false);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(1, request);

        // Assert
        _mockSubjectRepo.Verify(r => r.Update(subject), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenSubjectDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSubjectRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Subject?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(99, new UpdateSubjectRequest { Name = "X" });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Subject not found");
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateNameExistsInSchool_ShouldThrowBadRequestException()
    {
        // Arrange
        var subject = CreateValidSubject();
        _mockSubjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _mockSubjectRepo.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subject, bool>>>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(1, new UpdateSubjectRequest { Name = "Duplicate" });

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // --- Factory methods ---

    private static Subject CreateValidSubject() => new()
    {
        Id = 1,
        Name = "Mathematics",
        SchoolId = 1
    };

    private static CreateSubjectRequest CreateValidRequest() => new()
    {
        Name = "Mathematics"
    };
}
