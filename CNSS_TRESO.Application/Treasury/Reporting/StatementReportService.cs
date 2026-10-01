using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury.Reporting;

public interface IStatementReportService
{
    Task<StatementReport> GenerateAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? journalCode,
        string? costCenter,
        IReadOnlyCollection<string>? allowedJournalNumbers = null,
        CancellationToken cancellationToken = default);
}

public sealed record StatementReport(
    DateOnly FromDate,
    DateOnly ToDate,
    string? JournalCode,
    string? JournalLabel,
    string? CostCenter,
    string? CostCenterLabel,
    IReadOnlyList<StatementReportRow> Rows,
    decimal TotalReceipts,
    decimal TotalExpenses,
    decimal Net,
    int Count);

public sealed record StatementReportRow(
    int Number,
    DateOnly? Date,
    string Journal,
    string DocumentCode,
    int DocumentNumber,
    string? Description,
    string? Direction,
    decimal Receipt,
    decimal Expense,
    string? Currency,
    string? CostCenter,
    string? Account,
    string? Beneficiary,
    bool Validated);

public sealed class StatementReportService : IStatementReportService
{
    private readonly ITreasuryMovementRepository _movements;
    private readonly IJournalRepository _journals;
    private readonly ICostCenterDirectory _costCenters;

    public StatementReportService(ITreasuryMovementRepository movements, IJournalRepository journals, ICostCenterDirectory costCenters)
    {
        _movements = movements;
        _journals = journals;
        _costCenters = costCenters;
    }

    public async Task<StatementReport> GenerateAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? journalCode,
        string? costCenter,
        IReadOnlyCollection<string>? allowedJournalNumbers = null,
        CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
        journalCode = string.IsNullOrWhiteSpace(journalCode) ? null : journalCode.Trim();
        costCenter = string.IsNullOrWhiteSpace(costCenter) ? null : costCenter.Trim();

        var filter = new MovementFilter(
            FromDate: fromDate,
            ToDate: toDate,
            Journal: journalCode,
            CostCenter: costCenter,
            JournalNumbers: allowedJournalNumbers,
            Take: 100_000);

        var data = await _movements.SearchAsync(filter, cancellationToken);
        var ordered = data.OrderBy(m => m.MovementDate).ThenBy(m => m.Journal).ThenBy(m => m.Number).ToList();

        decimal totalR = 0m, totalD = 0m;
        var rows = new List<StatementReportRow>(ordered.Count);
        foreach (var m in ordered)
        {
            var amt = m.Amount ?? 0m;
            var isR = string.Equals(m.Direction, "R", StringComparison.OrdinalIgnoreCase);
            var receipt = isR ? amt : 0m;
            var expense = isR ? 0m : amt;
            totalR += receipt;
            totalD += expense;
            rows.Add(new StatementReportRow(
                m.Number,
                m.MovementDate,
                m.Journal ?? string.Empty,
                m.DocumentCode ?? string.Empty,
                m.DocumentNumber,
                m.Description,
                m.Direction,
                receipt,
                expense,
                m.CurrencyCode,
                m.CostCenter,
                m.Account,
                m.Beneficiary ?? m.Payer,
                m.Validated == true));
        }

        string? journalLabel = null;
        if (!string.IsNullOrWhiteSpace(journalCode))
        {
            var j = await _journals.GetByNumberAsync(journalCode, cancellationToken);
            journalLabel = j?.Label;
        }

        var costCenterLabel = await _costCenters.ResolveLabelAsync(costCenter, cancellationToken);
        return new StatementReport(
            fromDate, toDate, journalCode, journalLabel, costCenter, costCenterLabel,
            rows, totalR, totalD, totalR - totalD, ordered.Count);
    }
}
