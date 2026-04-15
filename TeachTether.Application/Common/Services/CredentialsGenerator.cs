using System.Text.RegularExpressions;
using PasswordGenerator;
using TeachTether.Application.Common.Interfaces;
using Unidecode.NET;

namespace TeachTether.Application.Common.Services;

public class CredentialsGenerator : ICredentialsGenerator
{
    public string GeneratePassword()
    {
        var pwd = new Password(12);
        return pwd.Next();
    }

    public string GenerateUsername(string firstName, string? middleName, string lastName)
    {
        var rawUsername =
            $"{SafeSlice(firstName, 20)}.{(string.IsNullOrWhiteSpace(middleName) ? "" : middleName[0] + ".")}{SafeSlice(lastName, 20)}";

        var transliterated = rawUsername.Unidecode();
        var sanitized = Regex.Replace(transliterated.ToLowerInvariant(), "[^a-z.]", "");

        var uniqueSuffix = Guid.NewGuid().ToString("N")[..6];
        return $"{sanitized}_{uniqueSuffix}";
    }

    private static string SafeSlice(string input, int length)
    {
        return input.Length > length ? input[..length] : input;
    }
}