namespace CNSS_TRESO.Application.Treasury;

public interface IComptaRepository
{
    Task<IReadOnlyList<TiersOption>> ListTiersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountOption>> ListAccountsAsync(string tiersCode, string? costCenter, CancellationToken cancellationToken = default);
}

public sealed record TiersOption(string Code, string Designation);
public sealed record AccountOption(string Code, string Label, string GeneralAccount);
