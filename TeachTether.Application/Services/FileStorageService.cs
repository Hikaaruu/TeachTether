using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;

namespace TeachTether.Application.Services;

public class FileStorageService(IFileStorageRepository repo) : IFileStorageService
{
    private const long MaxFileSize = 50 * 1024 * 1024;

    private static readonly string[] AllowedTypes =
    [
        "image/jpeg", "image/png", "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    private static readonly Regex SafeName = new(@"[^a-zA-Z0-9_.\-]", RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    private static readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"];
    private readonly IFileStorageRepository _repo = repo;

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        return _repo.DeleteAsync(relativePath, ct);
    }

    public async Task<Stream> DownloadAsync(string relativePath, CancellationToken ct = default)
    {
        var stream = await _repo.OpenReadAsync(relativePath, ct);
        return stream;
    }

    public async Task<string> UploadAsync(IFormFile file, int threadId, CancellationToken ct = default)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            throw new InvalidOperationException("Invalid file size.");

        if (!AllowedTypes.Contains(file.ContentType))
            throw new InvalidOperationException("File type not allowed.");

        var originalName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalName) || originalName.Contains("..") || originalName.Contains('/') ||
            originalName.Contains('\\'))
            throw new InvalidOperationException("Invalid file name.");

        var ext = Path.GetExtension(originalName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
            throw new InvalidOperationException("Invalid or disallowed file extension.");

        var baseName = Path.GetFileNameWithoutExtension(originalName);

        if (string.IsNullOrWhiteSpace(baseName) || SafeName.IsMatch(baseName))
            throw new InvalidOperationException("File name contains invalid characters.");

        var guid = Guid.NewGuid().ToString("N");
        var storedName = $"{guid}{ext}";

        var relative = Path.Combine("uploads", threadId.ToString(), storedName)
            .Replace("\\", "/");

        await using var stream = file.OpenReadStream();
        await _repo.SaveAsync(relative, stream, ct);

        return relative;
    }
}