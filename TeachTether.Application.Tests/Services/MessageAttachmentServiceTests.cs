using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class MessageAttachmentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IFileStorageService> _mockFileStorageService = new();
    private readonly Mock<IMessageRepository> _mockMessageRepo = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMessageAttachmentRepository> _mockAttachmentRepo = new();
    private readonly MessageAttachmentService _sut;

    public MessageAttachmentServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.MessageAttachments).Returns(_mockAttachmentRepo.Object);
        _sut = new MessageAttachmentService(
            _mockUnitOfWork.Object,
            _mockFileStorageService.Object,
            _mockMessageRepo.Object,
            _mockMapper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenMessageExistsAndFileIsValid_ShouldUploadStoreAndReturnResponse()
    {
        // Arrange
        var message = new Message { Id = 1, ThreadId = 5, SenderUserId = "u1", IsRead = false, SentAt = DateTime.UtcNow };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("report.pdf");
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.Length).Returns(1024);

        var response = new MessageAttachmentResponse
        {
            Id = 1,
            MessageId = 1,
            FileName = "report.pdf",
            FileType = "application/pdf",
            DownloadUrl = "/files/report.pdf"
        };

        _mockMessageRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(message);
        _mockFileStorageService.Setup(s => s.UploadAsync(mockFile.Object, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploads/5/report.pdf");
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<MessageAttachmentResponse>(It.IsAny<MessageAttachment>())).Returns(response);

        // Act
        var result = await _sut.CreateAsync(1, mockFile.Object);

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockAttachmentRepo.Verify(r => r.AddAsync(It.Is<MessageAttachment>(a =>
            a.MessageId == 1 &&
            a.FileName == "report.pdf" &&
            a.FileType == "application/pdf" &&
            a.FileSizeBytes == 1024)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenMessageDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(99, new Mock<IFormFile>().Object);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Message not found");
    }

    // --- GetFileByIdAsync ---

    [Fact]
    public async Task GetFileByIdAsync_WhenAttachmentExists_ShouldReturnFileDownloadModel()
    {
        // Arrange
        var attachment = new MessageAttachment
        {
            Id = 1,
            MessageId = 1,
            FileName = "doc.pdf",
            FileType = "application/pdf",
            FileUrl = "uploads/5/doc.pdf"
        };
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockAttachmentRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(attachment);
        _mockFileStorageService.Setup(s => s.DownloadAsync("uploads/5/doc.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        // Act
        var result = await _sut.GetFileByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("doc.pdf");
        result.ContentType.Should().Be("application/pdf");
        result.Content.Should().BeSameAs(stream);
    }

    [Fact]
    public async Task GetFileByIdAsync_WhenAttachmentDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockAttachmentRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MessageAttachment?)null);

        // Act
        Func<Task> act = async () => await _sut.GetFileByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Attachment not found");
    }
}
