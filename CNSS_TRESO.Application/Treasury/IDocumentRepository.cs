using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> ListAsync(string? journalType = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> ListByJournalAsync(string journalNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
}
