using CNSS_TRESO.Application.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class SqlUserSecurityRepository : IUserSecurityRepository
{
    private readonly string _connectionString;
    private bool _initialized;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    public SqlUserSecurityRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CnssTreso") ?? throw new InvalidOperationException("Connection string 'CnssTreso' is missing.");
    }

    public async Task<UserSecurityProfile?> GetAsync(string username, CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT PasswordHash, IsActive, FailedLoginAttempts, LockoutEndUtc FROM dbo.UserSecurityProfile WHERE Username = @username";
        command.Parameters.AddWithValue("@username", username.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new UserSecurityProfile(username.Trim(), reader.GetString(0), reader.GetBoolean(1), reader.GetInt32(2), reader.IsDBNull(3) ? null : reader.GetDateTime(3));
    }

    public async Task SaveAsync(UserSecurityProfile profile, CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"MERGE dbo.UserSecurityProfile AS target
USING (SELECT @username AS Username) AS source ON target.Username = source.Username
WHEN MATCHED THEN UPDATE SET PasswordHash = @hash, IsActive = @active, FailedLoginAttempts = @failures, LockoutEndUtc = @lockout
WHEN NOT MATCHED THEN INSERT (Username, PasswordHash, IsActive, FailedLoginAttempts, LockoutEndUtc) VALUES (@username, @hash, @active, @failures, @lockout);";
        command.Parameters.AddWithValue("@username", profile.Username);
        command.Parameters.AddWithValue("@hash", profile.PasswordHash);
        command.Parameters.AddWithValue("@active", profile.IsActive);
        command.Parameters.AddWithValue("@failures", profile.FailedLoginAttempts);
        command.Parameters.AddWithValue("@lockout", (object?)profile.LockoutEndUtc ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureTableAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return;
        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = @"IF OBJECT_ID(N'dbo.UserSecurityProfile', N'U') IS NULL
CREATE TABLE dbo.UserSecurityProfile (
    Username nvarchar(20) NOT NULL PRIMARY KEY,
    PasswordHash nvarchar(256) NOT NULL,
    IsActive bit NOT NULL CONSTRAINT DF_UserSecurityProfile_IsActive DEFAULT 1,
    FailedLoginAttempts int NOT NULL CONSTRAINT DF_UserSecurityProfile_Failures DEFAULT 0,
    LockoutEndUtc datetime2 NULL);";
            await command.ExecuteNonQueryAsync(cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }
}