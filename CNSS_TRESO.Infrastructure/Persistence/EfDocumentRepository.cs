using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfDocumentRepository : IDocumentRepository
{
    private const string DocumentsQuery = """
        SELECT [num],
               COALESCE([BED obligatoire], '0') AS [BED obligatoire],
               [Code document],
               [Monnaie],
               [Sens mouvement],
               [Journal],
               [Type journal],
               [Libellé document],
               [Type mouvement],
               [Numéro document]
        FROM dbo.[Table documents]
        """;

    private readonly CnssTresoDbContext _dbContext;

    public EfDocumentRepository(CnssTresoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Document>> ListAsync(string? journalType = null, CancellationToken cancellationToken = default)
    {
        var q = _dbContext.Documents.FromSqlRaw(DocumentsQuery).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(journalType))
        {
            var t = journalType.Trim();
            q = q.Where(d => d.JournalType == t);
        }
        return await q
            .OrderBy(d => d.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> ListByJournalAsync(string journalNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(journalNumber)) return Array.Empty<Document>();
        var j = journalNumber.Trim();
        return await _dbContext.Documents.FromSqlRaw(DocumentsQuery).AsNoTracking()
            .Where(d => d.Journal == j)
            .OrderBy(d => d.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
