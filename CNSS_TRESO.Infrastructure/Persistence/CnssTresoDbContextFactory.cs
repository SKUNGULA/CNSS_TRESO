using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class CnssTresoDbContextFactory : IDesignTimeDbContextFactory<CnssTresoDbContext>
{
    public CnssTresoDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("CnssTreso")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=CNSS_TRESO;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<CnssTresoDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new CnssTresoDbContext(optionsBuilder.Options);
    }
}
