using CNSS_TRESO.Application.Budget;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class SqlBudgetExecutionRepository : IBudgetExecutionRepository
{
    private readonly string _connectionString;

    public SqlBudgetExecutionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Budget")
            ?? throw new InvalidOperationException("Connection string 'Budget' is missing.");
    }

    public async Task<int?> ResolveCentreGestionIdAsync(string codeCentreGestion, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codeCentreGestion)) return null;
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 CentreGestionId FROM dbo.ParamCentreGestion WHERE CodeCentreGestion = @c";
        cmd.Parameters.Add(new SqlParameter("@c", codeCentreGestion.Trim()));
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result is null || result is DBNull) return null;
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdatePaymentAsync(string numeroTitre, int centreGestionId, decimal mntP, decimal mntTp, DateOnly paymentDate, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE dbo.ExecutionBudget
                            SET MontantPaye = @mntTp,
                                MontantPayeTP = @mntP,
                                DatePaiement = @dt,
                                EtatTreso = 1
                            WHERE NumeroTitre = @num AND CentreGestionId = @cf";
        cmd.Parameters.Add(new SqlParameter("@mntTp", mntTp));
        cmd.Parameters.Add(new SqlParameter("@mntP", mntP));
        cmd.Parameters.Add(new SqlParameter("@dt", paymentDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new SqlParameter("@num", numeroTitre));
        cmd.Parameters.Add(new SqlParameter("@cf", centreGestionId));
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> RevertPaymentAsync(string numeroTitre, int centreGestionId, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE dbo.ExecutionBudget
                            SET MontantPaye = 0,
                                MontantPayeTP = 0,
                                DatePaiement = NULL,
                                EtatTreso = 0
                            WHERE NumeroTitre = @num AND CentreGestionId = @cf";
        cmd.Parameters.Add(new SqlParameter("@num", numeroTitre));
        cmd.Parameters.Add(new SqlParameter("@cf", centreGestionId));
        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
