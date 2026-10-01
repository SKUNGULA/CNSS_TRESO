using CNSS_TRESO.Application.Exercises;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Exercises;

public sealed class ExerciseConnectionStringBuilder : IExerciseConnectionStringBuilder
{
    private readonly string _template;

    public ExerciseConnectionStringBuilder(IConfiguration configuration)
    {
        _template = configuration.GetConnectionString("CnssTreso")
            ?? throw new InvalidOperationException("Connection string 'CnssTreso' missing.");
    }

    public string BuildFor(string databaseSuffix)
    {
        var builder = new SqlConnectionStringBuilder(_template);
        var baseName = builder.InitialCatalog;

        if (!string.IsNullOrWhiteSpace(databaseSuffix))
        {
            var prefix = ExtractPrefix(baseName);
            // If the value already looks like a full database name (starts with the prefix),
            // use it as-is; otherwise treat it as a suffix and append to the prefix.
            builder.InitialCatalog = databaseSuffix.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? databaseSuffix
                : prefix + databaseSuffix;
        }

        return builder.ConnectionString;
    }

    private static string ExtractPrefix(string baseName)
    {
        if (string.IsNullOrEmpty(baseName)) return "BDDTRESO";
        var idx = baseName.Length;
        while (idx > 0 && char.IsDigit(baseName[idx - 1])) idx--;
        return baseName.Substring(0, idx);
    }
}
