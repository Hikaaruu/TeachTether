namespace TeachTether.Application.Common.Interfaces;

public interface ICredentialsGenerator
{
    string GenerateUsername(string firstName, string? middleName, string lastName);
    string GeneratePassword();
}