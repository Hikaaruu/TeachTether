using AutoMapper;
using TeachTether.Application.Common;
using TeachTether.Application.Common.Exceptions;
using TeachTether.Application.Common.Interfaces;
using TeachTether.Application.Common.Models;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ICredentialsGenerator _credentialsGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(ICredentialsGenerator credentialsGenerator, IUserRepository userRepository, IMapper mapper, IJwtTokenGenerator jwtTokenGenerator)
        {
            _credentialsGenerator = credentialsGenerator;
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<(OperationResult, User)> RegisterAsync(RegisterRequest request)
        {
            var user = _mapper.Map<User>(request);
            user.UserType = UserType.SchoolOwner;

            return (await _userRepository.CreateAsync(user, request.Password), user);
        }

        public async Task<(User, string password)> CreateAsync(CreateUserDto createUserDto, UserType userType)
        {
            var user = _mapper.Map<User>(createUserDto);
            user.UserType = userType;

            do
            {
                user.UserName = _credentialsGenerator
                    .GenerateUsername(user.FirstName, user.MiddleName, user.LastName);
            } while (await _userRepository.FindByUserNameAsync(user.UserName) is not null);

            var password = _credentialsGenerator.GeneratePassword();
            var result = await _userRepository.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new Exception("Failed to create user");

            return (user, password);
        }

        public async Task UpdateAsync(string userId, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            _mapper.Map(updateUserDto, user);

            var result = await _userRepository.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to update user");

        }

        public async Task<string?> TryLoginAsync(LoginRequest request)
        {
            var user = await _userRepository.FindByUserNameAsync(request.UserName);
            if (user is null)
                return null;

            var isValid = await _userRepository.CheckPasswordAsync(user.Id!, request.Password);
            if (!isValid)
                return null;

            return await _jwtTokenGenerator.GenerateJwtTokenAsync(user);
        }

        public async Task<User> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("User not found");

            return user;
        }

        public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await _userRepository.GetByIdsAsync(ids.Distinct());
        }
    }
}
