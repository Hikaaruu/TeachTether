using System.Security.Claims;
using TeachTether.Application.Common;
using TeachTether.Application.DTOs;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<(OperationResult Result, string? Token)> RegisterAsync(RegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
        Task<UserInfoResponse?> GetCurrentUserInfoAsync(ClaimsPrincipal user);

    }
}
