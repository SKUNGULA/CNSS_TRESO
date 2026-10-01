using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfAuditEntryRepository : IAuditEntryRepository
{
    private readonly CnssTresoDbContext _dbContext;

    public EfAuditEntryRepository(CnssTresoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.AuditEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int max, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditEntries
            .OrderByDescending(x => x.OccurredAt)
            .Take(max <= 0 ? 100 : max)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEntry>> SearchAsync(AuditFilter filter, CancellationToken cancellationToken = default)
    {
        var q = _dbContext.AuditEntries.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Username))
        {
            var u = filter.Username.Trim();
            q = q.Where(x => EF.Functions.Like(x.Username, "%" + u + "%"));
        }
        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var a = filter.Action.Trim();
            q = q.Where(x => EF.Functions.Like(x.Action, "%" + a + "%"));
        }
        if (filter.FromUtc.HasValue) q = q.Where(x => x.OccurredAt >= filter.FromUtc.Value);
        if (filter.ToUtc.HasValue) q = q.Where(x => x.OccurredAt < filter.ToUtc.Value);
        return await q.OrderByDescending(x => x.OccurredAt)
            .Take(filter.Max <= 0 ? 500 : filter.Max)
            .ToListAsync(cancellationToken);
    }
}
