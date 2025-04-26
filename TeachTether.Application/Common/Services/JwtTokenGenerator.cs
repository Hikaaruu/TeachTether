using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TeachTether.Application.Common.Interfaces;
using TeachTether.Application.Interfaces.Repositories;
using TeachTether.Application.Settings;
using TeachTether.Domain.Entities;

namespace TeachTether.Application.Common.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
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
