using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using TeachTether.Application.DTOs;
using TeachTether.Application.Validators;

namespace TeachTether.Application.Tests.Validators;

public class CreateMessageRequestValidatorTests
{
    private readonly CreateMessageRequestValidator _sut = new();

    private static Mock<IFormFile> CreateValidFileMock(
        string fileName = "document.pdf",
        string contentType = "application/pdf",
        long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        return mockFile;
    }

    [Fact]
    public void Should_NotHaveErrors_When_Content_IsProvided_AndNoAttachments()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = "Hello, how are you?",
            Attachments = []
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveErrors_When_ContentIsNull_AndValidAttachmentIsProvided()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = null,
            Attachments = [CreateValidFileMock().Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Root-level rule ---

    [Fact]
    public void Should_HaveError_When_ContentIsNull_AndAttachments_IsEmpty()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = null,
            Attachments = []
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    // --- Content (conditional: only validated when non-null) ---

    [Fact]
    public void Should_HaveError_When_Content_IsNonNull_AndEmpty()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = "",
            Attachments = []
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_HaveError_When_Content_ExceedsMaxLength()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = new string('a', 19_501),
            Attachments = []
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_HaveError_When_Content_ContainsDangerousScript()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = "<script>alert('xss')</script>",
            Attachments = []
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_HaveError_When_Content_ContainsJavascriptKeyword()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = "javascript:alert(1)",
            Attachments = []
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Should_NotHaveError_When_Content_IsNull_AndConditionSkipsValidation()
    {
        // Arrange
        var model = new CreateMessageRequest
        {
            Content = null,
            Attachments = [CreateValidFileMock().Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    // --- Attachment: Length ---

    [Fact]
    public void Should_HaveError_When_Attachment_ExceedsMaxFileSize()
    {
        // Arrange
        const long overLimit = 51 * 1024 * 1024; // 51 MB > 50 MB max
        var mockFile = CreateValidFileMock(length: overLimit);
        var model = new CreateMessageRequest
        {
            Content = "Message with large file",
            Attachments = [mockFile.Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "Attachments[0].Length");
    }

    // --- Attachment: ContentType ---

    [Fact]
    public void Should_HaveError_When_Attachment_HasUnsupportedContentType()
    {
        // Arrange
        var mockFile = CreateValidFileMock(contentType: "text/plain");
        var model = new CreateMessageRequest
        {
            Content = "Message with unsupported file",
            Attachments = [mockFile.Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == "Attachments[0].ContentType");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("application/msword")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void Should_NotHaveError_When_Attachment_HasSupportedContentType(string contentType)
    {
        // Arrange
        var mockFile = CreateValidFileMock(contentType: contentType);
        var model = new CreateMessageRequest
        {
            Content = "Message",
            Attachments = [mockFile.Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Attachments[0].ContentType");
    }

    // --- Attachment: FileName (path traversal) ---

    [Fact]
    public void Should_HaveError_When_Attachment_FileName_ContainsPathTraversal()
    {
        // Arrange
        var mockFile = CreateValidFileMock(fileName: "../secrets/evil.pdf");
        var model = new CreateMessageRequest
        {
            Content = "Message with unsafe filename",
            Attachments = [mockFile.Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Attachments[0].FileName" &&
            e.ErrorMessage.Contains("path traversal"));
    }

    // --- Attachment: FileName (safe filename pattern) ---

    [Fact]
    public void Should_HaveError_When_Attachment_FileName_ContainsInvalidCharacters()
    {
        // Arrange
        var mockFile = CreateValidFileMock(fileName: "file<with>bad|chars.pdf");
        var model = new CreateMessageRequest
        {
            Content = "Message",
            Attachments = [mockFile.Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("invalid or potentially unsafe"));
    }

    [Fact]
    public void Should_NotHaveErrors_When_Attachment_FileName_IsValid()
    {
        // Arrange
        var mockFile = CreateValidFileMock(fileName: "my-report_2024.pdf");
        var model = new CreateMessageRequest
        {
            Content = "Message",
            Attachments = [mockFile.Object]
        };

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorMessage.Contains("unsafe"));
    }
}
