using Microsoft.AspNetCore.Http;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, int threadId, CancellationToken ct = default);
        Task<Stream> DownloadAsync(string relativePath, CancellationToken ct = default);
        Task DeleteAsync(string relativePath, CancellationToken ct = default);
    }
}
