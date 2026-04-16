using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Interfaces.Services.DeletionHelpers;
using TeachTether.Application.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Tests.Services;

public class MessageServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IMessageAttachmentService> _mockAttachmentService = new();
    private readonly Mock<IMessageDeletionHelper> _mockDeletionHelper = new();
    private readonly Mock<IMessageRepository> _mockMessageRepo = new();
    private readonly Mock<IMessageAttachmentRepository> _mockAttachmentRepo = new();
    private readonly MessageService _sut;

    public MessageServiceTests()
    {
        _mockUnitOfWork.Setup(u => u.Messages).Returns(_mockMessageRepo.Object);
        _mockUnitOfWork.Setup(u => u.MessageAttachments).Returns(_mockAttachmentRepo.Object);
        _sut = new MessageService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockUserService.Object,
            _mockAttachmentService.Object,
            _mockDeletionHelper.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WhenRequestHasNoAttachments_ShouldAddMessageAndSaveOnce()
    {
        // Arrange
        var request = new CreateMessageRequest { Content = "Hello", Attachments = [] };
        var message = CreateValidMessage();
        var response = new MessageResponse { Id = 1, ThreadId = 5, SenderUserId = "u1" };

        _mockMapper.Setup(m => m.Map<Message>(request)).Returns(message);
        _mockMapper.Setup(m => m.Map<MessageResponse>(message)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, threadId: 5, userId: "u1");

        // Assert
        result.Should().BeEquivalentTo(response);
        _mockMessageRepo.Verify(r => r.AddAsync(message), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockAttachmentService.Verify(s => s.CreateAsync(It.IsAny<int>(), It.IsAny<IFormFile>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestHasAttachments_ShouldCallAttachmentServicePerFile()
    {
        // Arrange
        var file1 = new Mock<IFormFile>().Object;
        var file2 = new Mock<IFormFile>().Object;
        var request = new CreateMessageRequest { Content = "With files", Attachments = [file1, file2] };
        var message = CreateValidMessage();
        var response = new MessageResponse { Id = 1, ThreadId = 5, SenderUserId = "u1" };
        var attachmentResponse = new MessageAttachmentResponse
        {
            Id = 1,
            MessageId = 1,
            FileName = "file.pdf",
            FileType = "application/pdf",
            DownloadUrl = "/file"
        };

        _mockMapper.Setup(m => m.Map<Message>(request)).Returns(message);
        _mockMapper.Setup(m => m.Map<MessageResponse>(message)).Returns(response);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockAttachmentService.Setup(s => s.CreateAsync(message.Id, It.IsAny<IFormFile>()))
            .ReturnsAsync(attachmentResponse);

        // Act
        var result = await _sut.CreateAsync(request, threadId: 5, userId: "u1");

        // Assert
        _mockAttachmentService.Verify(s => s.CreateAsync(message.Id, It.IsAny<IFormFile>()), Times.Exactly(2));
        result.Attachments.Should().HaveCount(2);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenCalled_ShouldDelegateToMessageDeletionHelper()
    {
        // Arrange
        _mockDeletionHelper.Setup(h => h.DeleteMessageAsync(4)).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(4);

        // Assert
        _mockDeletionHelper.Verify(h => h.DeleteMessageAsync(4), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenMessageExists_ShouldReturnResponseWithAttachments()
    {
        // Arrange
        var message = CreateValidMessage();
        var attachment = new MessageAttachment { Id = 1, MessageId = 1, FileName = "doc.pdf", FileType = "application/pdf", FileUrl = "uploads/5/doc.pdf" };
        var response = new MessageResponse { Id = 1, ThreadId = 5, SenderUserId = "u1" };
        var attachmentResponses = new List<MessageAttachmentResponse>
        {
            new() { Id = 1, MessageId = 1, FileName = "doc.pdf", FileType = "application/pdf", DownloadUrl = "/d" }
        };

        _mockMessageRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(message);
        _mockAttachmentRepo.Setup(r => r.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MessageAttachment, bool>>>()))
            .ReturnsAsync(new List<MessageAttachment> { attachment });
        _mockMapper.Setup(m => m.Map<MessageResponse>(message)).Returns(response);
        _mockMapper.Setup(m => m.Map<List<MessageAttachmentResponse>>(It.IsAny<IEnumerable<MessageAttachment>>()))
            .Returns(attachmentResponses);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Attachments.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMessageDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("message not found");
    }

    // --- ReadAsync ---

    [Fact]
    public async Task ReadAsync_WhenMessageExists_ShouldMarkAsReadAndSave()
    {
        // Arrange
        var message = CreateValidMessage();
        message.IsRead = false;

        _mockMessageRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(message);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.ReadAsync(1);

        // Assert
        message.IsRead.Should().BeTrue();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReadAsync_WhenMessageDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Message?)null);

        // Act
        Func<Task> act = async () => await _sut.ReadAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("message not found");
    }

    // --- Factory methods ---

    private static Message CreateValidMessage() => new()
    {
        Id = 1,
        ThreadId = 5,
        SenderUserId = "u1",
        Content = "Hello",
        SentAt = DateTime.UtcNow,
        IsRead = false
    };
}
