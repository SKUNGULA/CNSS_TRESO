using CNSS_TRESO.Application.Security;
using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface ITreasuryMovementService
{
    Task<(bool IsSuccess, string? Error, int? Number)> CreateMovementAsync(CreateMovementRequest request, CallerContext caller, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreasuryMovement>> SearchAsync(MovementFilter filter, CancellationToken cancellationToken = default);
    Task<TreasuryMovement?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> UpdateDraftAsync(int number, CreateMovementRequest request, CallerContext caller, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> DeleteDraftAsync(int number, string reason, CallerContext caller, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> ValidateAsync(int number, CallerContext caller, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> ValidateGroupAsync(string journal, DateOnly movementDate, CallerContext caller, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> DevalidateAsync(int number, string reason, CallerContext caller, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> DevalidateGroupAsync(string journal, DateOnly movementDate, string reason, CallerContext caller, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> ListDocumentsAsync(string? journalType = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> ListDocumentsByJournalAsync(string journalNumber, CancellationToken cancellationToken = default);
}

public sealed record CreateMovementRequest(
    string Journal,
    string DocumentCode,
    int DocumentNumber,
    DateOnly MovementDate,
    decimal Amount,
    decimal? Rate,
    string CurrencyCode,
    string? Description,
    string? Direction,
    string? MovementType,
    string? Account,
    string? AccountType,
    string? Payer,
    string? Beneficiary,
    int? PieceNumber = null,
    string? CheckNumber = null,
    string? Bank = null);
