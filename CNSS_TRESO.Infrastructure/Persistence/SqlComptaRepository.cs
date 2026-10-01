using CNSS_TRESO.Application.Treasury;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class SqlComptaRepository : IComptaRepository
{
    private readonly string _connectionString;

    public SqlComptaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Compta")
            ?? throw new InvalidOperationException("Connection string 'Compta' is missing.");
    }

    public async Task<IReadOnlyList<TiersOption>> ListTiersAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Code, [Désignation] FROM dbo.CategorieCpte WHERE Code <> 'C' AND Code <> 'L' ORDER BY [Désignation]";
        var list = new List<TiersOption>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            list.Add(new TiersOption(r.GetString(0).Trim(), r.GetString(1).Trim()));
        }
        return list;
    }

    public async Task<IReadOnlyList<AccountOption>> ListAccountsAsync(string tiersCode, string? costCenter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tiersCode)) return Array.Empty<AccountOption>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT CodeScpte, Libelle, CompteGeneral
FROM dbo.Sous_Compte
WHERE Mouvement = 1
  AND Tiers = @tiers
  AND IndiceExigible = '0'
  AND LEFT(CodeScpte, 3) <> '415'
  AND LEFT(CodeScpte, 5) <> '41120'
  AND ( CentreFrais = @cf
        OR ((CodeScpte LIKE '5%' OR CodeScpte LIKE '4%') AND (CentreFrais IS NULL OR CentreFrais = '')) )
ORDER BY CompteGeneral, CodeScpte";
        cmd.Parameters.Add(new SqlParameter("@tiers", tiersCode.Trim()));
        cmd.Parameters.Add(new SqlParameter("@cf", (object?)(costCenter?.Trim()) ?? string.Empty));
        var list = new List<AccountOption>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            list.Add(new AccountOption(
                r.GetString(0).Trim(),
                r.IsDBNull(1) ? string.Empty : r.GetString(1).Trim(),
                r.IsDBNull(2) ? string.Empty : r.GetString(2).Trim()));
        }
        return list;
    }
}
