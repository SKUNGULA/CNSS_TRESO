using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Application.DTOs;

namespace CNSS_TRESO.Application.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSecurityRepository _securityRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditTrailService _audit;

    public AuthenticationService(IUserRepository userRepository, IUserSecurityRepository securityRepository, IPasswordHasher passwordHasher, IAuditTrailService audit)
    {
        _userRepository = userRepository;
        _securityRepository = securityRepository;
        _passwordHasher = passwordHasher;
        _audit = audit;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.PlainPassword))
        {
            return new AuthenticationResult(false, "Identifiant ou mot de passe invalide.", null, null, null, null, null);
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null)
        {
            await _audit.RecordAsync(request.Username, "Auth.Failure", "User", null, "Compte introuvable", cancellationToken);
            return new AuthenticationResult(false, "Identifiant ou mot de passe invalide.", null, null, null, null, null);
        }

        var security = await _securityRepository.GetAsync(user.Username, cancellationToken);
        if (security is not null && (!security.IsActive || security.LockoutEndUtc > DateTime.UtcNow))
        {
            await _audit.RecordAsync(user.Username, "Auth.Failure", "User", user.Id.ToString(), security.IsActive ? "Compte verrouille" : "Compte desactive", cancellationToken);
            return new AuthenticationResult(false, "Identifiant ou mot de passe invalide.", null, null, null, null, null);
        }

        var isValid = security is null
            ? string.Equals(request.PlainPassword, user.Password, StringComparison.Ordinal)
            : _passwordHasher.Verify(request.PlainPassword, security.PasswordHash);
        if (!isValid)
        {
            var failures = (security?.FailedLoginAttempts ?? 0) + 1;
            DateTime? lockedUntil = failures >= 5 ? DateTime.UtcNow.AddMinutes(15) : null;
            await _securityRepository.SaveAsync(new UserSecurityProfile(
                user.Username,
                security?.PasswordHash ?? _passwordHasher.Hash(user.Password),
                security?.IsActive ?? true,
                lockedUntil.HasValue ? 0 : failures,
                lockedUntil), cancellationToken);
            await _audit.RecordAsync(user.Username, "Auth.Failure", "User", user.Id.ToString(), "Mot de passe invalide", cancellationToken);
            return new AuthenticationResult(false, "Identifiant ou mot de passe invalide.", null, null, null, null, null);
        }

        await _securityRepository.SaveAsync(new UserSecurityProfile(user.Username, security?.PasswordHash ?? _passwordHasher.Hash(request.PlainPassword), security?.IsActive ?? true, 0, null), cancellationToken);

        await _audit.RecordAsync(user.Username, "Auth.Success", "User", user.Id.ToString(), null, cancellationToken);
        return new AuthenticationResult(
            true,
            null,
            user.Id,
            user.Username,
            user.FullName,
            user.AccessLevel,
            user.CostCenter
        );
    }
}
