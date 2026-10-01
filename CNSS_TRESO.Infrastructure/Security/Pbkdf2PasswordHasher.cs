using System.Security.Cryptography;
using CNSS_TRESO.Application.Contracts;

namespace CNSS_TRESO.Infrastructure.Security;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword)) throw new ArgumentException("Password cannot be empty.", nameof(plainPassword));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string plainPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword)) return false;

        var parts = hashedPassword.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations)) return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
