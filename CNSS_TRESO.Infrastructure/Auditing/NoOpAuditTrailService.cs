using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Domain.Auditing;

namespace CNSS_TRESO.Infrastructure.Auditing;

// Aucune table d'audit dans la base héritée — implémentation no-op.
public sealed class NoOpAuditTrailService : IAuditTrailService
{
    public Task RecordAsync(string username, string action, string entityType, string? entityId, string? details, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<AuditEntry>> ListRecentAsync(int max = 200, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>());

    public Task<IReadOnlyList<AuditEntry>> SearchAsync(AuditFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>());
}
