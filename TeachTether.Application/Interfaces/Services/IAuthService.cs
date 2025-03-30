using TeachTether.Application.Common;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<OperationResult> RegisterAsync(RegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
    }
}
