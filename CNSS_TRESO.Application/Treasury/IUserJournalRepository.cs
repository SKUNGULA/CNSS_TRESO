namespace CNSS_TRESO.Application.Treasury;

public interface IUserJournalRepository
{
    Task<IReadOnlyList<UserJournal>> ListAsync(string username, string costCenter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserJournal>> ListByCostCenterAsync(string costCenter, CancellationToken cancellationToken = default);
    Task<UserJournal?> GetAsync(string journalNumber, CancellationToken cancellationToken = default);
    Task ReplaceAssignmentsAsync(string username, IReadOnlyCollection<string> journalNumbers, CancellationToken cancellationToken = default);
}

public sealed record UserJournal(string Number, string Label, string? Currency, string? MovementType, int? NextPiece, string? JournalType);
