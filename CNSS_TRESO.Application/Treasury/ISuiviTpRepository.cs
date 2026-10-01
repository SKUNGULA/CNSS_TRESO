namespace CNSS_TRESO.Application.Treasury;

public interface ISuiviTpRepository
{
    Task<int> MarkFinalValidatedAsync(string numTp, string costCenter, DateOnly date, string? userName, CancellationToken cancellationToken = default);
    Task<int> ClearFinalValidationAsync(string numTp, string costCenter, CancellationToken cancellationToken = default);
    Task<SuiviTpStatus?> GetStatusAsync(string numTp, string costCenter, CancellationToken cancellationToken = default);
}

public sealed record SuiviTpStatus(string NumTp, string CostCenter, string? ValideFinal, DateOnly? DateValideFinal);
