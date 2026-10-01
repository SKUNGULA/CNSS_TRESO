using CNSS_TRESO.Domain.Auditing;

namespace CNSS_TRESO.Application.Auditing;

public interface IAuditEntryRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int max, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEntry>> SearchAsync(AuditFilter filter, CancellationToken cancellationToken = default);
}

public sealed record AuditFilter(string? Username, string? Action, DateTime? FromUtc, DateTime? ToUtc, int Max = 500);
