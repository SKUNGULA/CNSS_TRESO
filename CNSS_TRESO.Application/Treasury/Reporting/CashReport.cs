namespace CNSS_TRESO.Application.Treasury.Reporting;

public sealed record CashReport(
    DateOnly FromDate,
    DateOnly ToDate,
    string JournalCode,
    string? JournalLabel,
    string? JournalType,
    string? CostCenter,
    string? CostCenterLabel,
    IReadOnlyList<CashReportRow> Rows,
    decimal OpeningBalance,
    decimal TotalReceipts,
    decimal TotalExpenses,
    decimal ClosingBalance,
    int Count,
    CashReportSignatories Signatories
);

public sealed record CashReportSignatories(
    string? LeftName,
    string? LeftTitle,
    string? OfficerName,
    string? OfficerTitle,
    string? RightName,
    string? RightTitle,
    string? CenterName,
    string? CenterTitle
)
{
    public static CashReportSignatories Empty { get; } = new(null, null, null, null, null, null, null, null);
}
