using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Application.Contracts;

namespace CNSS_TRESO.Application.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;
    private readonly IAuditTrailService _audit;

    public PermissionService(IPermissionRepository repository, IAuditTrailService audit)
    {
        _repository = repository;
        _audit = audit;
    }

    public async Task<IReadOnlyList<ModulePermissionView>> GetUserModulesAsync(string username, CancellationToken cancellationToken = default)
    {
        var modules = await _repository.GetAllModulesAsync(cancellationToken);
        var granted = (await _repository.GetUserAccessAsync(username, cancellationToken))
            .Select(x => x.ModuleNumber)
            .ToHashSet();

        return modules
            .Select(m => new ModulePermissionView(m.Number, m.Name, granted.Contains(m.Number)))
            .ToList();
    }

    public async Task GrantAsync(string username, int moduleNumber, CancellationToken cancellationToken = default)
    {
        await _repository.GrantAsync(username, moduleNumber, cancellationToken);
        await _audit.RecordAsync(username, "Permission.Grant", "Module", moduleNumber.ToString(), $"User={username}", cancellationToken);
    }

    public async Task RevokeAsync(string username, int moduleNumber, CancellationToken cancellationToken = default)
    {
        await _repository.RevokeAsync(username, moduleNumber, cancellationToken);
        await _audit.RecordAsync(username, "Permission.Revoke", "Module", moduleNumber.ToString(), $"User={username}", cancellationToken);
    }
}
