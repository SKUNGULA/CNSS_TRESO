using System.Data;
using CNSS_TRESO.Application.Exercises;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Exercises;

public sealed class SqlExerciseCatalog : IExerciseCatalog
{
    private readonly string _connectionString;

    public SqlExerciseCatalog(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CnssTreso")
            ?? throw new InvalidOperationException("Connection string 'CnssTreso' is missing.");
    }

    public async Task<IReadOnlyList<ExerciseInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT name FROM sys.databases
            WHERE name LIKE 'BDDTRESO%'
              AND name NOT LIKE '%[_]%' ESCAPE '\'
            ORDER BY name DESC";

        var result = new List<ExerciseInfo>();
        await using var cnx = new SqlConnection(_connectionString);
        await cnx.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, cnx);
        await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await rd.ReadAsync(cancellationToken))
        {
            var name = rd.GetString(0);
            var suffix = name.Length > "BDDTRESO".Length ? name.Substring("BDDTRESO".Length) : "";
            int year;
            if (string.IsNullOrEmpty(suffix))
            {
                year = DateTime.Now.Year;
            }
            else if (int.TryParse(suffix, out var twoDigit))
            {
                year = twoDigit < 50 ? 2000 + twoDigit : 1900 + twoDigit;
            }
            else
            {
                continue;
            }
            result.Add(new ExerciseInfo(year, name));
        }
        return result;
    }
}
