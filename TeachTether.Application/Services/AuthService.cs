using AutoMapper;
using System.Security.Claims;
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
        private readonly IMapper _mapper;

        public AuthService(
            IUserService userService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(OperationResult Result, string? Token)> RegisterAsync(RegisterRequest request)
        {
            (var result, var user) = await _userService.RegisterAsync(request);

            if (!result.Succeeded)
                return (result, null);


            var schoolOwner = new SchoolOwner
            {
                UserId = user.Id!,
            };
            await _unitOfWork.SchoolOwners.AddAsync(schoolOwner);
            await _unitOfWork.SaveChangesAsync();

            var token = await _userService.TryLoginAsync(new LoginRequest
            {
                UserName = request.UserName,
                Password = request.Password
            });

            return (result, token);
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            return await _userService.TryLoginAsync(request);
        }

        public async Task<UserInfoResponse?> GetCurrentUserInfoAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var dbUser = await _userService.GetByIdAsync(userId);
            if (dbUser == null) return null;

            var entityId = dbUser.UserType switch
            {
                UserType.Student => (await _unitOfWork.Students.GetByUserIdAsync(userId))?.Id,
                UserType.Teacher => (await _unitOfWork.Teachers.GetByUserIdAsync(userId))?.Id,
                UserType.Guardian => (await _unitOfWork.Guardians.GetByUserIdAsync(userId))?.Id,
                UserType.SchoolAdmin => (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(userId))?.Id,
                UserType.SchoolOwner => (await _unitOfWork.SchoolOwners.GetByUserIdAsync(userId))?.Id,
                _ => null
            };

            var userInfo = _mapper.Map<UserInfoResponse>(dbUser);
            userInfo.EntityId = entityId!.Value;
            return userInfo;
        }
    }
}
