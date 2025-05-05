using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TeachTether.Application.Common;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;
using TeachTether.Infrastructure.Persistence.Data;

namespace TeachTether.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UserRepository(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<User?> GetByIdAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser is null)
                return null;

            return MapToDomainUser(appUser);
        }

        public async Task<OperationResult> CreateAsync(User user, string password)
        {
            var appUser = MapToApplicationUser(user);
            var identityResult = await _userManager.CreateAsync(appUser, password);

            if (identityResult.Succeeded)
            {
                user.Id = appUser.Id;
                return OperationResult.Success();
            }

            return OperationResult.Failure(identityResult.Errors.Select(e => e.Description));
        }

        public async Task<OperationResult> UpdateAsync(User user)
        {
            var appUser = await _userManager.FindByIdAsync(user.Id!);

            if (appUser is null)
                return OperationResult.Failure(["User not found."]);

            appUser.FirstName = user.FirstName;
            appUser.LastName = user.LastName;
            appUser.MiddleName = user.MiddleName;
            appUser.Sex = user.Sex;

            var identityResult = await _userManager.UpdateAsync(appUser);

            return identityResult.Succeeded
                ? OperationResult.Success()
                : OperationResult.Failure(identityResult.Errors.Select(e => e.Description));
        }

        public async Task<User?> FindByUserNameAsync(string userName)
        {
            var appUser = await _userManager.FindByNameAsync(userName);
            return appUser is null ? null : MapToDomainUser(appUser);
        }

        public async Task<bool> CheckPasswordAsync(string userId, string password)
        {
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser is null)
                return false;

            return await _userManager.CheckPasswordAsync(appUser, password);
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser is null)
                return Enumerable.Empty<Claim>();

            return await _userManager.GetClaimsAsync(appUser);
        }

        public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var appUsers = await _userManager.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            return appUsers.Select(MapToDomainUser);
        }



        private static ApplicationUser MapToApplicationUser(User user)
        {
            return new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Sex = user.Sex,
                UserType = user.UserType
            };
        }

        private static User MapToDomainUser(ApplicationUser user)
        {
            return new User
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Sex = user.Sex,
                UserType = user.UserType
            };
        }

    }
}
