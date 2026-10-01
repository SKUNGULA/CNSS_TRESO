using CNSS_TRESO.Domain.Entities;

namespace CNSS_TRESO.Application.Contracts;

public sealed record ModulePermissionView(int Number, string? Name, bool Granted);

public interface IPermissionService
{
    Task<IReadOnlyList<ModulePermissionView>> GetUserModulesAsync(string username, CancellationToken cancellationToken = default);
    Task GrantAsync(string username, int moduleNumber, CancellationToken cancellationToken = default);
    Task RevokeAsync(string username, int moduleNumber, CancellationToken cancellationToken = default);
}
