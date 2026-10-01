using CNSS_TRESO.Domain.Auditing;

namespace CNSS_TRESO.Application.Auditing;

public sealed class AuditTrailService : IAuditTrailService
{
    private readonly IAuditEntryRepository _repository;

    public AuditTrailService(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public Task RecordAsync(string username, string action, string entityType, string? entityId, string? details, CancellationToken cancellationToken = default)
    {
        var entry = new AuditEntry(Guid.NewGuid(), DateTime.UtcNow, username, action, entityType, entityId, details);
        return _repository.AddAsync(entry, cancellationToken);
    }

    public Task<IReadOnlyList<AuditEntry>> ListRecentAsync(int max = 200, CancellationToken cancellationToken = default)
        => _repository.GetRecentAsync(max, cancellationToken);

    public Task<IReadOnlyList<AuditEntry>> SearchAsync(AuditFilter filter, CancellationToken cancellationToken = default)
        => _repository.SearchAsync(filter, cancellationToken);
}
