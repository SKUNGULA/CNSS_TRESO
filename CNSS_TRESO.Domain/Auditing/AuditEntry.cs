namespace CNSS_TRESO.Domain.Auditing;

public sealed class AuditEntry
{
    public Guid Id { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Username { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? Details { get; private set; }

    private AuditEntry()
    {
        Username = string.Empty;
        Action = string.Empty;
        EntityType = string.Empty;
    }

    public AuditEntry(Guid id, DateTime occurredAt, string username, string action, string entityType, string? entityId, string? details)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("Entity type is required.", nameof(entityType));

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        OccurredAt = occurredAt;
        Username = username.Trim();
        Action = action.Trim();
        EntityType = entityType.Trim();
        EntityId = entityId?.Trim();
        Details = details;
    }
}
