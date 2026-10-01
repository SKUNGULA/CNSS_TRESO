using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Application.DTOs;
using CNSS_TRESO.Domain.Entities;

namespace CNSS_TRESO.Application.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSecurityRepository _securityRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditTrailService _audit;

    public UserManagementService(IUserRepository userRepository, IUserSecurityRepository securityRepository, IPasswordHasher passwordHasher, IAuditTrailService audit)
    {
        _userRepository = userRepository;
        _securityRepository = securityRepository;
        _passwordHasher = passwordHasher;
        _audit = audit;
    }

    public async Task<(bool IsSuccess, string? Error)> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.PlainPassword) || string.IsNullOrWhiteSpace(request.CostCenter))
        {
            return (false, "Tous les champs sont obligatoires.");
        }

        var existing = await _userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (existing is not null)
        {
            return (false, "Cet identifiant existe deja.");
        }

        var passwordHash = _passwordHasher.Hash(request.PlainPassword);
        var user = new User(
            Guid.NewGuid(),
            request.Username,
            request.FullName,
            passwordHash,
            request.AccessLevel,
            request.CostCenter,
            true
        );

        await _userRepository.AddAsync(user, cancellationToken);
        await _securityRepository.SaveAsync(new UserSecurityProfile(user.Username, passwordHash, true, 0, null), cancellationToken);
        await _audit.RecordAsync(request.Username, "User.Create", "User", user.Id.ToString(), $"Niveau={request.AccessLevel} CF={request.CostCenter}", cancellationToken);
        return (true, null);
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var mappedUsers = users
            .OrderBy(u => u.Username)
            .Select(async user => MapUser(user, await _securityRepository.GetAsync(user.Username, cancellationToken)))
            .ToArray();
        return await Task.WhenAll(mappedUsers);
    }

    public async Task<UserDto?> GetUserAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        var user = await _userRepository.GetByUsernameAsync(username.Trim(), cancellationToken);
        return user is null ? null : MapUser(user, await _securityRepository.GetAsync(user.Username, cancellationToken));
    }

    public async Task<(bool IsSuccess, string? Error)> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Matricule) || string.IsNullOrWhiteSpace(request.CostCenter))
            return (false, "Identifiant, matricule et centre de frais sont obligatoires.");

        var user = await _userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !string.Equals(user.Username, request.Username.Trim(), StringComparison.OrdinalIgnoreCase))
            return (false, "Utilisateur introuvable.");

        user.UpdateProfile(request.Matricule, request.AccessLevel, request.CostCenter);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _audit.RecordAsync(user.Username, "User.Update", "User", user.Id.ToString(), $"Matricule={user.Matricule} Niveau={user.AccessLevel} CF={user.CostCenter}", cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? Error)> SetUserActiveAsync(string username, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (user is null || !string.Equals(user.Username, username.Trim(), StringComparison.OrdinalIgnoreCase)) return (false, "Utilisateur introuvable.");

        var current = await _securityRepository.GetAsync(user.Username, cancellationToken);
        await _securityRepository.SaveAsync(new UserSecurityProfile(user.Username, current?.PasswordHash ?? _passwordHasher.Hash(user.Password), isActive, 0, null), cancellationToken);
        await _audit.RecordAsync(user.Username, isActive ? "User.Activate" : "User.Deactivate", "User", user.Id.ToString(), null, cancellationToken);
        return (true, null);
    }

    private static UserDto MapUser(User user, UserSecurityProfile? security) =>
        new(user.Id, user.Username, user.FullName, user.AccessLevel, user.CostCenter, security?.IsActive ?? true, user.Matricule);
}
