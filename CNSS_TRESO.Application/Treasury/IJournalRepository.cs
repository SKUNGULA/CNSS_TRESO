using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface IJournalRepository
{
    Task<IReadOnlyList<Journal>> ListAsync(CancellationToken cancellationToken = default);
    Task<Journal?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task AddAsync(Journal journal, CancellationToken cancellationToken = default);
}

public interface ISignatoryRepository
{
    Task<Signatory?> GetByJournalAsync(string journal, CancellationToken cancellationToken = default);
    Task UpdateAsync(string journal, string? leftName, string? leftTitle, string? officerName, string? officerTitle, string? rightName, string? rightTitle, string? centerName, string? centerTitle, CancellationToken cancellationToken = default);
}
