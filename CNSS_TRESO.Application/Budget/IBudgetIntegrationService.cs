using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Budget;

public interface IBudgetIntegrationService
{
    Task<BudgetSettlementResult> SettleAsync(TreasuryMovement movement, DateOnly paymentDate, CancellationToken cancellationToken = default);
    Task<BudgetSettlementResult> RevertAsync(TreasuryMovement movement, CancellationToken cancellationToken = default);
}

public sealed record BudgetSettlementResult(bool IsSuccess, string? Message, int? CentreGestionId = null, decimal? MontantPaye = null, decimal? MontantPayeTP = null);
