using CNSS_TRESO.Domain.Entities;

namespace CNSS_TRESO.Application.Contracts;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Module>> GetAllModulesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserModuleAccess>> GetUserAccessAsync(string username, CancellationToken cancellationToken = default);
    Task GrantAsync(string username, int moduleNumber, CancellationToken cancellationToken = default);
    Task RevokeAsync(string username, int moduleNumber, CancellationToken cancellationToken = default);
}
