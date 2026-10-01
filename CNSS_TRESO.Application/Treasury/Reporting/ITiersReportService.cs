namespace CNSS_TRESO.Application.Treasury.Reporting;

public interface ITiersReportService
{
    Task<TiersReport> GenerateAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? costCenter,
        string? journalCode,
        string? direction,
        string? tiersFilter,
        IReadOnlyCollection<string>? allowedJournalNumbers = null,
        CancellationToken cancellationToken = default);
}
