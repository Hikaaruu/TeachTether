using System.Text.RegularExpressions;
using FluentValidation;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Validators;

public class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    private const long MaxFileSize = 50 * 1024 * 1024;

    private static readonly string[] AllowedTypes =
    [
        "image/jpeg", "image/png", "image/gif",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    private static readonly Regex DangerousContentPattern = new(
        @"<script.*?>.*?</script.*?>|[<>]|\b(eval|alert|javascript:|onerror|onload)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)
    );

    private static readonly Regex SafeFilenamePattern = new(
        @"^[\w\-. ]{1,255}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100)
    );

    public CreateMessageRequestValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Content) ||
                x.Attachments?.Count > 0)
            .WithMessage("Message must contain text or at least one attachment.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message cannot be empty.")
            .MaximumLength(19500)
            .WithMessage("Content cannot exceed 19,500 characters.")
            .Must(content => content == null || !DangerousContentPattern.IsMatch(content))
            .WithMessage("Message contains potentially dangerous characters or scripts.")
            .When(x => x.Content is not null);


        RuleForEach(x => x.Attachments)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.Length)
                    .LessThanOrEqualTo(MaxFileSize)
                    .WithMessage($"Each file must be ≤ {MaxFileSize / (1024 * 1024)} MB.");

                file.RuleFor(f => f.ContentType)
                    .Must(t => AllowedTypes.Contains(t))
                    .WithMessage($"Files must be one of: {string.Join(", ", AllowedTypes)}.");

                file.RuleFor(f => Path.GetFileName(f.FileName))
                    .Must(name => SafeFilenamePattern.IsMatch(name))
                    .WithMessage("Attachment filename is invalid or potentially unsafe.");

                file.RuleFor(f => f.FileName)
                    .Must(name => !name.Contains(".."))
                    .WithMessage("Attachment filename must not contain '..' (path traversal).");
            });
    }
}