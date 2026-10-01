namespace CNSS_TRESO.Application.Treasury.Reporting;

public interface ICashReportService
{
    Task<CashReport> GenerateAsync(string journalCode, DateOnly fromDate, DateOnly toDate, string? costCenter, CancellationToken cancellationToken = default);
}
