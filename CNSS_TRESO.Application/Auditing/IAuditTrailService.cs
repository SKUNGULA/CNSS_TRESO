using CNSS_TRESO.Domain.Auditing;

namespace CNSS_TRESO.Application.Auditing;

public interface IAuditTrailService
{
    Task RecordAsync(string username, string action, string entityType, string? entityId, string? details, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> ListRecentAsync(int max = 200, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> SearchAsync(AuditFilter filter, CancellationToken cancellationToken = default);
}
