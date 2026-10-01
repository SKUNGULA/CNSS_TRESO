namespace CNSS_TRESO.Application.Treasury;

public interface ICostCenterDirectory
{
    Task<string?> ResolveLabelAsync(string? code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CostCenterOption>> ListAsync(CancellationToken cancellationToken = default);
}

public sealed record CostCenterOption(string Code, string Label);
