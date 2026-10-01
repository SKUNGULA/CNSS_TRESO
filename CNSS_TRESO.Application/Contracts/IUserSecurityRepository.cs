namespace CNSS_TRESO.Application.Contracts;

public interface IUserSecurityRepository
{
    Task<UserSecurityProfile?> GetAsync(string username, CancellationToken cancellationToken = default);
    Task SaveAsync(UserSecurityProfile profile, CancellationToken cancellationToken = default);
}

public sealed record UserSecurityProfile(
    string Username,
    string PasswordHash,
    bool IsActive,
    int FailedLoginAttempts,
    DateTime? LockoutEndUtc);