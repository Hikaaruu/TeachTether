using AutoMapper;
using TeachTether.Application.Common.Interfaces;
using TeachTether.Application.Common.Models;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Common.Services
{
    public class UserCreationService : IUserCreationService
    {
        private readonly ICredentialsGenerator _credentialsGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserCreationService(ICredentialsGenerator credentialsGenerator, IUserRepository userRepository, IMapper mapper)
        {
            _credentialsGenerator = credentialsGenerator;
            _userRepository = userRepository;
            _mapper = mapper;
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
    }
}
