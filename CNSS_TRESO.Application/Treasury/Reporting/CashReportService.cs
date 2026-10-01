using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury.Reporting;

public sealed class CashReportService : ICashReportService
{
    private readonly ITreasuryMovementRepository _movements;
    private readonly IJournalRepository _journals;
    private readonly ISignatoryRepository _signatories;
    private readonly ICostCenterDirectory _costCenters;

    public CashReportService(
        ITreasuryMovementRepository movements,
        IJournalRepository journals,
        ISignatoryRepository signatories,
        ICostCenterDirectory costCenters)
    {
        _movements = movements;
        _journals = journals;
        _signatories = signatories;
        _costCenters = costCenters;
    }

    public async Task<CashReport> GenerateAsync(string journalCode, DateOnly fromDate, DateOnly toDate, string? costCenter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(journalCode))
            throw new ArgumentException("Le journal est obligatoire pour générer le rapport.", nameof(journalCode));

        if (toDate < fromDate)
            (fromDate, toDate) = (toDate, fromDate);

        journalCode = journalCode.Trim();
        costCenter = string.IsNullOrWhiteSpace(costCenter) ? null : costCenter.Trim();

        var journal = await _journals.GetByNumberAsync(journalCode, cancellationToken);

        var sumRBefore = await _movements.SumAmountAsync(journalCode, costCenter, "R", fromDate, strictlyBefore: true, cancellationToken);
        var sumDBefore = await _movements.SumAmountAsync(journalCode, costCenter, "D", fromDate, strictlyBefore: true, cancellationToken);
        var opening = sumRBefore - sumDBefore;

        var filter = new MovementFilter(
            FromDate: fromDate,
            ToDate: toDate,
            Journal: journalCode,
            CostCenter: costCenter,
            Take: 100_000);

        var movements = await _movements.SearchAsync(filter, cancellationToken);
        var ordered = movements.OrderBy(m => m.MovementDate).ThenBy(m => m.Number).ToList();

        var rows = new List<CashReportRow>(ordered.Count + 1)
        {
            new(fromDate, journalCode, string.Empty, string.Empty, string.Empty, "Report", string.Empty,
                journal?.Currency ?? string.Empty,
                opening >= 0 ? opening : 0m,
                opening < 0 ? Math.Abs(opening) : 0m,
                opening,
                IsOpening: true)
        };

        decimal running = opening;
        decimal totalR = 0m;
        decimal totalD = 0m;

        foreach (var m in ordered)
        {
            var amount = m.Amount ?? 0m;
            var isReceipt = string.Equals(m.Direction, "R", StringComparison.OrdinalIgnoreCase);
            var receipt = isReceipt ? amount : 0m;
            var expense = isReceipt ? 0m : amount;
            running += receipt - expense;
            totalR += receipt;
            totalD += expense;

            rows.Add(new CashReportRow(
                m.MovementDate ?? fromDate,
                m.Journal ?? string.Empty,
                m.DocumentNumber.ToString(),
                m.PieceNumber?.ToString() ?? string.Empty,
                m.Account ?? string.Empty,
                m.Description ?? string.Empty,
                m.Beneficiary ?? m.Payer ?? string.Empty,
                m.CurrencyCode ?? string.Empty,
                receipt,
                expense,
                running));
        }

        var sig = await _signatories.GetByJournalAsync(journalCode, cancellationToken);
        var signatories = sig is null
            ? CashReportSignatories.Empty
            : new CashReportSignatories(
                sig.LeftName, sig.LeftTitle,
                sig.OfficerName, sig.OfficerTitle,
                sig.RightName, sig.RightTitle,
                sig.CenterName, sig.CenterTitle);

        var costCenterLabel = await _costCenters.ResolveLabelAsync(costCenter, cancellationToken);
        return new CashReport(
            fromDate, toDate,
            journalCode,
            journal?.Label,
            journal?.Type,
            costCenter,
            costCenterLabel,
            rows,
            opening, totalR, totalD, running,
            ordered.Count,
            signatories);
    }
}
