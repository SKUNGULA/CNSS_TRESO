using CNSS_TRESO.Domain.Auditing;
using CNSS_TRESO.Domain.Entities;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class CnssTresoDbContext : DbContext
{
    public CnssTresoDbContext(DbContextOptions<CnssTresoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<UserModuleAccess> UserModuleAccesses => Set<UserModuleAccess>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<Signatory> Signatories => Set<Signatory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ModuleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserModuleAccessConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ExchangeRateConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AuditEntryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.JournalConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SignatoryConfiguration());
    }
}
