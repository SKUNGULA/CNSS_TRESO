using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Budget;

public sealed class BudgetIntegrationService : IBudgetIntegrationService
{
    private readonly IBudgetExecutionRepository _budget;
    private readonly IExchangeRateRepository _rates;

    public BudgetIntegrationService(IBudgetExecutionRepository budget, IExchangeRateRepository rates)
    {
        _budget = budget;
        _rates = rates;
    }

    public async Task<BudgetSettlementResult> SettleAsync(TreasuryMovement movement, DateOnly paymentDate, CancellationToken cancellationToken = default)
    {
        var prep = await PrepareAsync(movement, cancellationToken);
        if (!prep.IsSuccess) return prep.Result!;

        var monnaie = (movement.CurrencyCode ?? string.Empty).Trim().ToUpperInvariant();
        var mntP = movement.Amount!.Value;
        decimal mntTp;

        if (monnaie == "CDF")
        {
            mntTp = mntP;
        }
        else
        {
            var rateDate = movement.MovementDate ?? paymentDate;
            var tauxCdf = await _rates.GetAmountAsync("CDF", rateDate, cancellationToken);
            var tauxJr = movement.Rate ?? 0m;
            if (tauxCdf is null or 0m || tauxJr == 0m)
                return new BudgetSettlementResult(false, $"Taux indisponible ({monnaie} le {rateDate:yyyy-MM-dd}).");
            mntTp = mntP * tauxCdf.Value / tauxJr;
        }

        var rows = await _budget.UpdatePaymentAsync(prep.NumeroTitre!, prep.CentreGestionId!.Value, mntP, mntTp, paymentDate, cancellationToken);
        if (rows == 0)
            return new BudgetSettlementResult(false, $"Aucune ligne ExecutionBudget pour titre {prep.NumeroTitre}/CF {prep.CentreGestionId}.");
        return new BudgetSettlementResult(true, $"{rows} ligne(s) BUDGET mise(s) à jour.", prep.CentreGestionId, mntP, mntTp);
    }

    public async Task<BudgetSettlementResult> RevertAsync(TreasuryMovement movement, CancellationToken cancellationToken = default)
    {
        var prep = await PrepareAsync(movement, cancellationToken);
        if (!prep.IsSuccess) return prep.Result!;

        var rows = await _budget.RevertPaymentAsync(prep.NumeroTitre!, prep.CentreGestionId!.Value, cancellationToken);
        return new BudgetSettlementResult(true, $"{rows} ligne(s) BUDGET ramenées à zéro.", prep.CentreGestionId);
    }

    private async Task<(bool IsSuccess, BudgetSettlementResult? Result, string? NumeroTitre, int? CentreGestionId)> PrepareAsync(TreasuryMovement movement, CancellationToken ct)
    {
        if (movement is null)
            return (false, new BudgetSettlementResult(false, "Mouvement obligatoire."), null, null);
        if (!string.Equals(movement.Direction, "D", StringComparison.OrdinalIgnoreCase))
            return (false, new BudgetSettlementResult(true, "Mouvement non concerné (sens != D)."), null, null);
        if (movement.PieceNumber is null or 0)
            return (false, new BudgetSettlementResult(true, "Mouvement sans n° pièce - pas de lien BUDGET."), null, null);
        if (string.IsNullOrWhiteSpace(movement.CostCenter))
            return (false, new BudgetSettlementResult(false, "Centre de frais absent sur le mouvement."), null, null);
        if (movement.Amount is null)
            return (false, new BudgetSettlementResult(false, "Montant absent."), null, null);

        var cfId = await _budget.ResolveCentreGestionIdAsync(movement.CostCenter!, ct);
        if (cfId is null)
            return (false, new BudgetSettlementResult(false, $"Aucun CentreGestionId pour {movement.CostCenter}."), null, null);

        return (true, null, movement.PieceNumber.Value.ToString(), cfId);
    }
}
