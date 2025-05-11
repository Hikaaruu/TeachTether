namespace TeachTether.Application.Interfaces.Repositories
{
    public interface IFileStorageRepository
    {
        Task SaveAsync(string relativePath, Stream source, CancellationToken ct = default);
        Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default);
        Task DeleteAsync(string relativePath, CancellationToken ct = default);
    }
}
