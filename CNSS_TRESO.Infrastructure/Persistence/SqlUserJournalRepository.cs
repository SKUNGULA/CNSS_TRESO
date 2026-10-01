using CNSS_TRESO.Application.Treasury;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class SqlUserJournalRepository : IUserJournalRepository
{
    private readonly string _connectionString;

    public SqlUserJournalRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CnssTreso")
            ?? throw new InvalidOperationException("Connection string 'CnssTreso' is missing.");
    }

    public async Task<IReadOnlyList<UserJournal>> ListAsync(string username, string costCenter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(costCenter))
            return Array.Empty<UserJournal>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT [Numéro journal], [Libellé journal], Monnaie, [Type Mouvement], [Numéro pièce], [Type journal]
FROM dbo.vChargementJournal
WHERE Utilisateur = @u AND CentreFrais = @cf
ORDER BY [Libellé journal]";
        cmd.Parameters.Add(new SqlParameter("@u", username.Trim()));
        cmd.Parameters.Add(new SqlParameter("@cf", costCenter.Trim()));
        var list = new List<UserJournal>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            list.Add(new UserJournal(
                r.GetString(0).Trim(),
                r.IsDBNull(1) ? string.Empty : r.GetString(1).Trim(),
                r.IsDBNull(2) ? null : r.GetString(2).Trim(),
                r.IsDBNull(3) ? null : r.GetString(3).Trim(),
                r.IsDBNull(4) ? null : r.GetInt32(4),
                r.IsDBNull(5) ? null : r.GetString(5).Trim()));
        }
        return list;
    }

    public async Task<IReadOnlyList<UserJournal>> ListByCostCenterAsync(string costCenter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(costCenter)) return Array.Empty<UserJournal>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT [Numéro journal], MAX([Libellé journal]), MAX(Monnaie), MAX([Type Mouvement]), MAX([Numéro pièce]), MAX([Type journal])
FROM dbo.vChargementJournal
WHERE CentreFrais = @cf
GROUP BY [Numéro journal]
ORDER BY MAX([Libellé journal])";
        cmd.Parameters.Add(new SqlParameter("@cf", costCenter.Trim()));
        var list = new List<UserJournal>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
        {
            list.Add(new UserJournal(
                r.GetString(0).Trim(),
                r.IsDBNull(1) ? string.Empty : r.GetString(1).Trim(),
                r.IsDBNull(2) ? null : r.GetString(2).Trim(),
                r.IsDBNull(3) ? null : r.GetString(3).Trim(),
                r.IsDBNull(4) ? null : r.GetInt32(4),
                r.IsDBNull(5) ? null : r.GetString(5).Trim()));
        }
        return list;
    }

    public async Task<UserJournal?> GetAsync(string journalNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(journalNumber)) return null;
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT TOP 1 [Numéro journal], [Libellé journal], Monnaie, [Type Mouvement], [Numéro pièce], [Type journal]
FROM dbo.vChargementJournal
WHERE [Numéro journal] = @n";
        cmd.Parameters.Add(new SqlParameter("@n", journalNumber.Trim()));
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await r.ReadAsync(cancellationToken)) return null;
        return new UserJournal(
            r.GetString(0).Trim(),
            r.IsDBNull(1) ? string.Empty : r.GetString(1).Trim(),
            r.IsDBNull(2) ? null : r.GetString(2).Trim(),
            r.IsDBNull(3) ? null : r.GetString(3).Trim(),
            r.IsDBNull(4) ? null : r.GetInt32(4),
            r.IsDBNull(5) ? null : r.GetString(5).Trim());
    }

    public async Task ReplaceAssignmentsAsync(string username, IReadOnlyCollection<string> journalNumbers, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("L'utilisateur est obligatoire.", nameof(username));

        var numbers = journalNumbers
            .Where(number => !string.IsNullOrWhiteSpace(number))
            .Select(number => number.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await conn.BeginTransactionAsync(cancellationToken);

        await using (var deleteCommand = conn.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM dbo.[Utilisateurs des journaux] WHERE Utilisateur = @user";
            deleteCommand.Parameters.Add(new SqlParameter("@user", username.Trim()));
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var number in numbers)
        {
            await using var insertCommand = conn.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = "INSERT INTO dbo.[Utilisateurs des journaux] (Utilisateur, Journal) VALUES (@user, @journal)";
            insertCommand.Parameters.Add(new SqlParameter("@user", username.Trim()));
            insertCommand.Parameters.Add(new SqlParameter("@journal", number));
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
