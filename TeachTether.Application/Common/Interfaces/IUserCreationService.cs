using TeachTether.Application.Common.Models;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Common.Interfaces
{
    public interface IUserCreationService
    {
        Task<(User, string password)> CreateAsync(CreateUserDto createUserDto, UserType userType);
    }
}
