using CNSS_TRESO.Application.Contracts;

namespace CNSS_TRESO.Infrastructure.Security;

// Legacy compatibility: mots de passe stockés en clair dans [Table utilisateurs].[Mot de passe]
public sealed class PlainPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => plainPassword ?? string.Empty;

    public bool Verify(string plainPassword, string hashedPassword)
        => string.Equals(plainPassword ?? string.Empty, hashedPassword ?? string.Empty, StringComparison.Ordinal);
}
