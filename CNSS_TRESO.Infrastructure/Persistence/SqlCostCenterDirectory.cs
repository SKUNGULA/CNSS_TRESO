using System.Collections.Concurrent;
using CNSS_TRESO.Application.Treasury;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class SqlCostCenterDirectory : ICostCenterDirectory
{
    private readonly string _connectionString;
    private static readonly ConcurrentDictionary<string, string?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public SqlCostCenterDirectory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Budget")
            ?? throw new InvalidOperationException("Connection string 'Budget' is missing.");
    }

    public async Task<string?> ResolveLabelAsync(string? code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var key = code.Trim();
        if (Cache.TryGetValue(key, out var cached)) return cached;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 CentreGestion FROM dbo.ParamCentreGestion WHERE CodeCentreGestion = @c";
        cmd.Parameters.Add(new SqlParameter("@c", key));
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var label = (result is null || result is DBNull) ? null : Convert.ToString(result);
        Cache[key] = label;
        return label;
    }

    public async Task<IReadOnlyList<CostCenterOption>> ListAsync(CancellationToken cancellationToken = default)
    {
        var centers = new List<CostCenterOption>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT CodeCentreGestion, CentreGestion
            FROM dbo.ParamCentreGestion
            WHERE CodeCentreGestion IS NOT NULL
            ORDER BY CodeCentreGestion";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader.GetString(0).Trim();
            var label = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
            centers.Add(new CostCenterOption(code, label));
        }
        return centers;
    }
}
