using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface ITreasuryMovementRepository
{
    Task<int> AddAsync(TreasuryMovement movement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreasuryMovement>> SearchAsync(MovementFilter filter, CancellationToken cancellationToken = default);
    Task<TreasuryMovement?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);
    Task UpdateValidationAsync(int number, bool validated, CancellationToken cancellationToken = default);
    Task<int> UpdateValidationByJournalAndDateAsync(string journal, DateOnly movementDate, bool validated, CancellationToken cancellationToken = default);
    Task UpdateDraftAsync(TreasuryMovement movement, CancellationToken cancellationToken = default);
    Task DeleteAsync(int number, CancellationToken cancellationToken = default);
    Task<decimal> SumAmountAsync(string? journal, string? costCenter, string direction, DateOnly upTo, bool strictlyBefore, CancellationToken cancellationToken = default);
}

public sealed record MovementFilter(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? Journal = null,
    string? DocumentCode = null,
    string? CostCenter = null,
    string? Direction = null,
    bool? OnlyUnvalidated = null,
    IReadOnlyCollection<string>? JournalNumbers = null,
    int Take = 200);
