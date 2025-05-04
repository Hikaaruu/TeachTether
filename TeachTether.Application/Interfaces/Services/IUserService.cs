using TeachTether.Application.Common;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<(OperationResult, User)> RegisterAsync(RegisterRequest request);
        Task<(User, string password)> CreateAsync(CreateUserDto createUserDto, UserType userType);
        Task UpdateAsync(string userId, UpdateUserDto updateUserDto);
        Task<string?> TryLoginAsync(LoginRequest request);
        Task<User> GetByIdAsync(string id);
        Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<string> ids);
    }
}
