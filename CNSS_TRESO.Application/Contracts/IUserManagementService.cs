using CNSS_TRESO.Application.DTOs;

namespace CNSS_TRESO.Application.Contracts;

public interface IUserManagementService
{
    Task<(bool IsSuccess, string? Error)> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> SetUserActiveAsync(string username, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserAsync(string username, CancellationToken cancellationToken = default);
}
