using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfJournalRepository : IJournalRepository
{
    private readonly CnssTresoDbContext _ctx;

    public EfJournalRepository(CnssTresoDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Journal>> ListAsync(CancellationToken cancellationToken = default)
        => await _ctx.Journals.AsNoTracking().OrderBy(j => j.Number).ToListAsync(cancellationToken);

    public Task<Journal?> GetByNumberAsync(string number, CancellationToken cancellationToken = default)
        => _ctx.Journals.AsNoTracking().FirstOrDefaultAsync(j => j.Number == number, cancellationToken);

    public async Task AddAsync(Journal journal, CancellationToken cancellationToken = default)
    {
        await _ctx.Journals.AddAsync(journal, cancellationToken);
        await _ctx.SaveChangesAsync(cancellationToken);
    }
}

public sealed class EfSignatoryRepository : ISignatoryRepository
{
    private readonly CnssTresoDbContext _ctx;

    public EfSignatoryRepository(CnssTresoDbContext ctx) => _ctx = ctx;

    public Task<Signatory?> GetByJournalAsync(string journal, CancellationToken cancellationToken = default)
        => _ctx.Signatories.AsNoTracking().FirstOrDefaultAsync(s => s.Journal == journal, cancellationToken);

    public async Task UpdateAsync(string journal, string? leftName, string? leftTitle, string? officerName, string? officerTitle, string? rightName, string? rightTitle, string? centerName, string? centerTitle, CancellationToken cancellationToken = default)
    {
        const string sql = @"MERGE [Table signateur] AS target
USING (SELECT @journal AS journal) AS source ON target.journal = source.journal
WHEN MATCHED THEN UPDATE SET
    [Signature de gauche] = @leftName, [Titre de gauche] = @leftTitle,
    [Signature du prepose] = @officerName, [Titre du prepose] = @officerTitle,
    [Signature de droite] = @rightName, [Titre de droite] = @rightTitle,
    [Signature de centre] = @centerName, [Titre de centre] = @centerTitle
WHEN NOT MATCHED THEN INSERT (journal, [Signature de gauche], [Titre de gauche], [Signature du prepose], [Titre du prepose], [Signature de droite], [Titre de droite], [Signature de centre], [Titre de centre])
VALUES (@journal, @leftName, @leftTitle, @officerName, @officerTitle, @rightName, @rightTitle, @centerName, @centerTitle);";

        await _ctx.Database.ExecuteSqlRawAsync(sql,
            new[]
            {
                new SqlParameter("@leftName", (object?)leftName ?? DBNull.Value),
                new SqlParameter("@leftTitle", (object?)leftTitle ?? DBNull.Value),
                new SqlParameter("@officerName", (object?)officerName ?? DBNull.Value),
                new SqlParameter("@officerTitle", (object?)officerTitle ?? DBNull.Value),
                new SqlParameter("@rightName", (object?)rightName ?? DBNull.Value),
                new SqlParameter("@rightTitle", (object?)rightTitle ?? DBNull.Value),
                new SqlParameter("@centerName", (object?)centerName ?? DBNull.Value),
                new SqlParameter("@centerTitle", (object?)centerTitle ?? DBNull.Value),
                new SqlParameter("@journal", journal),
            },
            cancellationToken);
    }
}
