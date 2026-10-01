namespace CNSS_TRESO.Application.Treasury.Reporting;

public sealed record CashReportRow(
    DateOnly MovementDate,
    string JournalCode,
    string DocumentNumber,
    string PieceNumber,
    string Account,
    string Description,
    string Beneficiary,
    string CurrencyCode,
    decimal AmountReceipt,
    decimal AmountExpense,
    decimal RunningBalance,
    bool IsOpening = false
);
