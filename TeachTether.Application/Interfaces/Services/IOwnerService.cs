namespace TeachTether.Application.Interfaces.Services
{
    public interface IOwnerService
    {
        Task<bool> ExistsAsync(int id);
    }
}
