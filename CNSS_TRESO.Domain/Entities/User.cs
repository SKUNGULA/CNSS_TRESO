using System.Security.Cryptography;
using System.Text;
using CNSS_TRESO.Domain.Enums;

namespace CNSS_TRESO.Domain.Entities;

public sealed class User
{
    public string Username { get; private set; } = string.Empty;
    public string Matricule { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public AccessLevel AccessLevel { get; private set; }
    public string CostCenter { get; private set; } = string.Empty;

    // Backward-compat computed properties for upper layers
    public Guid Id => DeterministicGuid(Username);
    public string FullName => Username;
    public string PasswordHash => Password;
    public bool IsActive => true;

    private User() { }

    public User(Guid id, string username, string fullName, string passwordHash, AccessLevel accessLevel, string costCenter, bool isActive)
        : this(username, passwordHash, accessLevel, costCenter, matricule: null)
    {
    }

    public User(string username, string password, AccessLevel accessLevel, string costCenter, string? matricule = null)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username requis.", nameof(username));
        Username = username.Trim();
        Password = password ?? string.Empty;
        AccessLevel = accessLevel;
        CostCenter = (costCenter ?? string.Empty).Trim();
        var mat = (matricule ?? username).Trim();
        Matricule = mat.Length > 10 ? mat.Substring(0, 10) : mat;
    }

    public void Deactivate() { }
    public void Activate() { }

    public void UpdateProfile(string matricule, AccessLevel accessLevel, string costCenter)
    {
        if (string.IsNullOrWhiteSpace(matricule)) throw new ArgumentException("Matricule requis.", nameof(matricule));
        if (string.IsNullOrWhiteSpace(costCenter)) throw new ArgumentException("Centre de frais requis.", nameof(costCenter));

        var trimmedMatricule = matricule.Trim();
        Matricule = trimmedMatricule.Length > 10 ? trimmedMatricule[..10] : trimmedMatricule;
        AccessLevel = accessLevel;
        CostCenter = costCenter.Trim();
    }

    private static Guid DeterministicGuid(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
        return new Guid(bytes);
    }
}
