using TeachTether.Domain.Entities;

namespace TeachTether.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateJwtTokenAsync(User user);
}