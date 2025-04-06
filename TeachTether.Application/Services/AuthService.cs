using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            IUserRepository userRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _jwtSettings = jwtSettings.Value;
            _userRepository = userRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
                await _unitOfWork.SchoolOwners.AddAsync(schoolOwner);
                await _unitOfWork.SaveChangesAsync();
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
                new (ClaimTypes.NameIdentifier, user.Id!),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (ClaimTypes.Name, user.UserName),
                new (ClaimTypes.GivenName, user.FirstName),
                new (ClaimTypes.Surname, user.LastName),
                new (ClaimTypes.Gender, user.Sex.ToString()),
                new (ClaimTypes.Role, user.UserType.ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.MiddleName))
            {
                claims.Add(new Claim("middle_name", user.MiddleName));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            var entityId = await GetEntityIdByUserTypeAsync(user);
            if (entityId != null)
            {
                claims.Add(new Claim("entity_id", entityId));
            }

            var userClaims = await _userRepository.GetClaimsAsync(user.Id!);
            claims.AddRange(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }

        private async Task<string?> GetEntityIdByUserTypeAsync(User user)
        {
            return user.UserType switch
            {
                UserType.Student => (await _unitOfWork.Students.GetByUserIdAsync(user.Id!))?.Id.ToString(),
                UserType.Teacher => (await _unitOfWork.Teachers.GetByUserIdAsync(user.Id!))?.Id.ToString(),
                UserType.Guardian => (await _unitOfWork.Guardians.GetByUserIdAsync(user.Id!))?.Id.ToString(),
                UserType.SchoolAdmin => (await _unitOfWork.SchoolAdmins.GetByUserIdAsync(user.Id!))?.Id.ToString(),
                UserType.SchoolOwner => (await _unitOfWork.SchoolOwners.GetByUserIdAsync(user.Id!))?.Id.ToString(),
                _ => null
            };
        }
    }
}
