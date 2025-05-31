using System.Security.Claims;
using TeachTether.Application.Common;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<OperationResult> CreateAsync(User user, string password);
    Task<OperationResult> UpdateAsync(User user);
    Task<OperationResult> DeleteAsync(string userId);
    Task<User?> FindByUserNameAsync(string userName);
    Task<User?> GetByIdAsync(string userId);
    Task<bool> CheckPasswordAsync(string userId, string password);
    Task<IEnumerable<Claim>> GetClaimsAsync(string userId);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<string> ids);
}