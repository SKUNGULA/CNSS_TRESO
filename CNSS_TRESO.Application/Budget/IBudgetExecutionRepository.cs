using CNSS_TRESO.Domain.Budget;

namespace CNSS_TRESO.Application.Budget;

public interface IBudgetExecutionRepository
{
    Task<int?> ResolveCentreGestionIdAsync(string codeCentreGestion, CancellationToken cancellationToken = default);
    Task<int> UpdatePaymentAsync(string numeroTitre, int centreGestionId, decimal mntP, decimal mntTp, DateOnly paymentDate, CancellationToken cancellationToken = default);
    Task<int> RevertPaymentAsync(string numeroTitre, int centreGestionId, CancellationToken cancellationToken = default);
}
