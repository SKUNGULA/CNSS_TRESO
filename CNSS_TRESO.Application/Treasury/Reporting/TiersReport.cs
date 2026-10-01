namespace CNSS_TRESO.Application.Treasury.Reporting;

public sealed record TiersReportRow(
    string TiersName,
    int Count,
    decimal TotalReceipts,
    decimal TotalExpenses,
    decimal Net,
    IReadOnlyList<TiersReportDetail> Details);

public sealed record TiersReportDetail(
    DateOnly MovementDate,
    string JournalCode,
    string DocumentCode,
    string DocumentNumber,
    string PieceNumber,
    string Account,
    string Description,
    string CurrencyCode,
    string Direction,
    decimal Amount);

public sealed record TiersReport(
    DateOnly FromDate,
    DateOnly ToDate,
    string? CostCenter,
    string? CostCenterLabel,
    string? JournalCode,
    string? Direction,
    string? TiersFilter,
    IReadOnlyList<TiersReportRow> Rows,
    int TotalCount,
    decimal GrandTotalReceipts,
    decimal GrandTotalExpenses,
    decimal GrandNet);
