using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury.Reporting;

public sealed class TiersReportService : ITiersReportService
{
    private readonly ITreasuryMovementRepository _movements;
    private readonly ICostCenterDirectory _costCenters;

    public TiersReportService(ITreasuryMovementRepository movements, ICostCenterDirectory costCenters)
    {
        _movements = movements;
        _costCenters = costCenters;
    }

    public async Task<TiersReport> GenerateAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string? costCenter,
        string? journalCode,
        string? direction,
        string? tiersFilter,
        IReadOnlyCollection<string>? allowedJournalNumbers = null,
        CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate) (fromDate, toDate) = (toDate, fromDate);
        costCenter = string.IsNullOrWhiteSpace(costCenter) ? null : costCenter.Trim();
        journalCode = string.IsNullOrWhiteSpace(journalCode) ? null : journalCode.Trim();
        direction = string.IsNullOrWhiteSpace(direction) ? null : direction.Trim().ToUpperInvariant();
        tiersFilter = string.IsNullOrWhiteSpace(tiersFilter) ? null : tiersFilter.Trim();

        var filter = new MovementFilter(
            FromDate: fromDate,
            ToDate: toDate,
            Journal: journalCode,
            CostCenter: costCenter,
            Direction: direction,
            JournalNumbers: allowedJournalNumbers,
            Take: 100_000);

        var movements = await _movements.SearchAsync(filter, cancellationToken);

        var filtered = movements
            .Where(m => tiersFilter is null ||
                        (m.Beneficiary?.Contains(tiersFilter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (m.Payer?.Contains(tiersFilter, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        // legacy : pour D on regroupe par Bénéficiaire, pour R on regroupe par Payeur
        static string TiersOf(TreasuryMovement m)
        {
            var isReceipt = string.Equals(m.Direction, "R", StringComparison.OrdinalIgnoreCase);
            var name = isReceipt ? (m.Payer ?? m.Beneficiary) : (m.Beneficiary ?? m.Payer);
            name = name?.Trim();
            return string.IsNullOrWhiteSpace(name) ? "(non renseigné)" : name!;
        }

        var groups = filtered
            .GroupBy(TiersOf)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rows = new List<TiersReportRow>(groups.Count);
        decimal grandR = 0m, grandD = 0m;
        int totalCount = 0;

        foreach (var g in groups)
        {
            var ordered = g.OrderBy(m => m.MovementDate).ThenBy(m => m.Number).ToList();
            decimal sumR = 0m, sumD = 0m;
            var details = new List<TiersReportDetail>(ordered.Count);
            foreach (var m in ordered)
            {
                var amount = m.Amount ?? 0m;
                var isR = string.Equals(m.Direction, "R", StringComparison.OrdinalIgnoreCase);
                if (isR) sumR += amount; else sumD += amount;
                details.Add(new TiersReportDetail(
                    m.MovementDate ?? fromDate,
                    m.Journal ?? string.Empty,
                    m.DocumentCode ?? string.Empty,
                    m.DocumentNumber.ToString(),
                    m.PieceNumber?.ToString() ?? string.Empty,
                    m.Account ?? string.Empty,
                    m.Description ?? string.Empty,
                    m.CurrencyCode ?? string.Empty,
                    m.Direction ?? string.Empty,
                    amount));
            }
            rows.Add(new TiersReportRow(g.Key, ordered.Count, sumR, sumD, sumR - sumD, details));
            grandR += sumR;
            grandD += sumD;
            totalCount += ordered.Count;
        }

        var costCenterLabel = await _costCenters.ResolveLabelAsync(costCenter, cancellationToken);
        return new TiersReport(
            fromDate, toDate, costCenter, costCenterLabel, journalCode, direction, tiersFilter,
            rows, totalCount, grandR, grandD, grandR - grandD);
    }
}
