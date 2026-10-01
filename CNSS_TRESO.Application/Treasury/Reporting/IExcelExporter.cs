using CNSS_TRESO.Domain.Auditing;

namespace CNSS_TRESO.Application.Treasury.Reporting;

public interface IExcelExporter
{
    byte[] ExportCashReport(CashReport report);
    byte[] ExportTiersReport(TiersReport report);
    byte[] ExportAuditEntries(IReadOnlyList<AuditEntry> entries, string? username, string? action, DateTime? fromUtc, DateTime? toUtc);
}

public interface IPdfExporter
{
    byte[] ExportCashReport(CashReport report);
}
