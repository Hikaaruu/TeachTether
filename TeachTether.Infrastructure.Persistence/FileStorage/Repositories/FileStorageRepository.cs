using Microsoft.Extensions.Options;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Infrastructure.Persistence.FileStorage.Common;

namespace TeachTether.Infrastructure.Persistence.FileStorage.Repositories;

public class FileStorageRepository(IOptions<FileStorageOptions> opt) : IFileStorageRepository
{
    private readonly string _root = opt.Value.RootPath;

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var full = Path.Combine(_root, relativePath);
        if (File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var full = Path.Combine(_root, relativePath);
        Stream s = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(s);
    }

    public async Task SaveAsync(string relativePath, Stream source, CancellationToken ct = default)
    {
        var full = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await using var fs = new FileStream(full, FileMode.Create, FileAccess.Write);
        await source.CopyToAsync(fs, ct);
    }
}