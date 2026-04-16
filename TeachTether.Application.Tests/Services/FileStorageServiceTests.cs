using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Services;

namespace TeachTether.Application.Tests.Services;

public class FileStorageServiceTests
{
    private readonly Mock<IFileStorageRepository> _mockRepo = new();
    private readonly FileStorageService _sut;

    public FileStorageServiceTests()
    {
        _sut = new FileStorageService(_mockRepo.Object);
    }

    // --- UploadAsync ---

    [Fact]
    public async Task UploadAsync_WhenFileIsValid_ShouldSaveAndReturnRelativePath()
    {
        // Arrange
        var file = CreateMockFile("document.pdf", "application/pdf", 1024);
        _mockRepo.Setup(r => r.SaveAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UploadAsync(file, threadId: 5);

        // Assert
        result.Should().StartWith("uploads/5/");
        result.Should().EndWith(".pdf");
        _mockRepo.Verify(r => r.SaveAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WhenFileLengthIsZero_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var file = CreateMockFile("document.pdf", "application/pdf", 0);

        // Act
        Func<Task> act = async () => await _sut.UploadAsync(file, threadId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid file size.");
    }

    [Fact]
    public async Task UploadAsync_WhenFileLargerThan50MB_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const long overSize = 50L * 1024 * 1024 + 1;
        var file = CreateMockFile("large.pdf", "application/pdf", overSize);

        // Act
        Func<Task> act = async () => await _sut.UploadAsync(file, threadId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid file size.");
    }

    [Fact]
    public async Task UploadAsync_WhenContentTypeNotAllowed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var file = CreateMockFile("script.exe", "application/octet-stream", 512);

        // Act
        Func<Task> act = async () => await _sut.UploadAsync(file, threadId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("File type not allowed.");
    }

    [Fact]
    public async Task UploadAsync_WhenFileNameContainsDotDot_ShouldThrowInvalidOperationException()
    {
        // Arrange
        // The service calls Path.GetFileName first, then checks for ".." in the result.
        // A name like "test..file.pdf" retains ".." after Path.GetFileName.
        var file = CreateMockFile("test..file.pdf", "application/pdf", 512);

        // Act
        Func<Task> act = async () => await _sut.UploadAsync(file, threadId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid file name.");
    }

    [Fact]
    public async Task UploadAsync_WhenExtensionNotAllowed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        // .bmp has application/pdf content type but disallowed extension
        var file = CreateMockFile("image.bmp", "application/pdf", 512);

        // Act
        Func<Task> act = async () => await _sut.UploadAsync(file, threadId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid or disallowed file extension.");
    }

    [Fact]
    public async Task UploadAsync_WhenBaseNameHasUnsafeCharacters_ShouldThrowInvalidOperationException()
    {
        // Arrange
        // Space in base name triggers the SafeName regex
        var file = CreateMockFile("my file.pdf", "application/pdf", 512);

        // Act
        Func<Task> act = async () => await _sut.UploadAsync(file, threadId: 5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("File name contains invalid characters.");
    }

    // --- DownloadAsync ---

    [Fact]
    public async Task DownloadAsync_WhenCalled_ShouldReturnStreamFromRepository()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        _mockRepo.Setup(r => r.OpenReadAsync("uploads/5/abc.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        // Act
        var result = await _sut.DownloadAsync("uploads/5/abc.pdf");

        // Assert
        result.Should().BeSameAs(stream);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToRepository()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteAsync("uploads/5/abc.pdf", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync("uploads/5/abc.pdf");

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync("uploads/5/abc.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Factory methods ---

    private static IFormFile CreateMockFile(string fileName, string contentType, long length)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return mockFile.Object;
    }
}
