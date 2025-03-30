using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TeachTether.Application.Common;
using TeachTether.Application.DTOs;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Interfaces.Services;
using TeachTether.Application.Settings;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;
        private readonly ISchoolOwnerRepository _schoolOwnerRepository;
        private readonly IMapper _mapper;

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            ISchoolOwnerRepository schoolOwnerRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _jwtSettings = jwtSettings.Value;
            _schoolOwnerRepository = schoolOwnerRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult> RegisterAsync(RegisterRequest request)
        {
            var user = _mapper.Map<User>(request);
            user.UserType = UserType.SchoolOwner;

            var result = await _userRepository.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var schoolOwner = new SchoolOwner
                {
                    UserId = user.Id!,
                };
                await _schoolOwnerRepository.AddAsync(schoolOwner);
            }

            return result;
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.FindByUserNameAsync(request.UserName);
            if (user is null)
                return null;

            var isValid = await _userRepository.CheckPasswordAsync(user.Id!, request.Password);
            if (!isValid)
                return null;

            return await GenerateJwtTokenAsync(user);
        }

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Gender, user.Sex.ToString()),
                new Claim(ClaimTypes.Role, user.UserType.ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.MiddleName))
            {
                claims.Add(new Claim("middle_name", user.MiddleName));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            var userClaims = await _userRepository.GetClaimsAsync(user.Id!);
            claims.AddRange(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(Convert.ToDouble(_jwtSettings.ExpireHours));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }
    }
}
