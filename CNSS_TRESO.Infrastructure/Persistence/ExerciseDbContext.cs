using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class ExerciseDbContext : DbContext
{
    public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options) : base(options)
    {
    }

    public DbSet<TreasuryMovement> Movements => Set<TreasuryMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.TreasuryMovementConfiguration());
    }
}
