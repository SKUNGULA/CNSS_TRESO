using CNSS_TRESO.Application.Treasury;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class SqlSuiviTpRepository : ISuiviTpRepository
{
    private readonly string _connectionString;

    public SqlSuiviTpRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SuiviTp")
            ?? throw new InvalidOperationException("Connection string 'SuiviTp' is missing.");
    }

    public async Task<int> MarkFinalValidatedAsync(string numTp, string costCenter, DateOnly date, string? userName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numTp) || string.IsNullOrWhiteSpace(costCenter)) return 0;
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE dbo.coco
                            SET ValideFinal = '1',
                                DateValideFinal = @dt,
                                UtilValideFinal = COALESCE(@u, UtilValideFinal)
                            WHERE NumTP = @num AND Cf = @cf";
        cmd.Parameters.Add(new SqlParameter("@dt", date.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new SqlParameter("@u", (object?)userName ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@num", numTp.Trim()));
        cmd.Parameters.Add(new SqlParameter("@cf", costCenter.Trim()));
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> ClearFinalValidationAsync(string numTp, string costCenter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numTp) || string.IsNullOrWhiteSpace(costCenter)) return 0;
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE dbo.coco
                            SET ValideFinal = '0',
                                DateValideFinal = NULL
                            WHERE NumTP = @num AND Cf = @cf";
        cmd.Parameters.Add(new SqlParameter("@num", numTp.Trim()));
        cmd.Parameters.Add(new SqlParameter("@cf", costCenter.Trim()));
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SuiviTpStatus?> GetStatusAsync(string numTp, string costCenter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numTp) || string.IsNullOrWhiteSpace(costCenter)) return null;
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 ValideFinal, DateValideFinal FROM dbo.coco WHERE NumTP = @num AND Cf = @cf";
        cmd.Parameters.Add(new SqlParameter("@num", numTp.Trim()));
        cmd.Parameters.Add(new SqlParameter("@cf", costCenter.Trim()));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var vf = reader.IsDBNull(0) ? null : reader.GetString(0)?.Trim();
        DateOnly? dv = reader.IsDBNull(1) ? null : DateOnly.FromDateTime(reader.GetDateTime(1));
        return new SuiviTpStatus(numTp, costCenter, vf, dv);
    }
}
