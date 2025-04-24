using PasswordGenerator;
using System.Text.RegularExpressions;
using TeachTether.Application.Common.Interfaces;

namespace TeachTether.Application.Common.Services
{
    public class CredentialsGenerator : ICredentialsGenerator
    {
        public string GeneratePassword()
        {
            var pwd = new Password(12);
            return pwd.Next();
        }

        public string GenerateUsername(string firstName, string? middleName, string lastName)
        {
            var baseUsername = $"{firstName[..20]}.{(string.IsNullOrWhiteSpace(middleName) ? "" : middleName[0] + ".")}{lastName[..20]}";
            baseUsername = Regex.Replace(baseUsername, @"[^a-z.]", "");

            var uniqueSuffix = Guid.NewGuid().ToString("N")[..6];
            return $"{baseUsername}_{uniqueSuffix}";
        }
    }
}
