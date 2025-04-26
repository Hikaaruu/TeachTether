using TeachTether.Application.Common;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public AuthService(
            IUserService userService,
            IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult> RegisterAsync(RegisterRequest request)
        {
            (var result, var user) = await _userService.RegisterAsync(request);

            if (result.Succeeded)
            {
                var schoolOwner = new SchoolOwner
                {
                    UserId = user.Id!,
                };
                await _unitOfWork.SchoolOwners.AddAsync(schoolOwner);
                await _unitOfWork.SaveChangesAsync();
            }

            return result;
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            return await _userService.TryLoginAsync(request);
        }
    }
}
