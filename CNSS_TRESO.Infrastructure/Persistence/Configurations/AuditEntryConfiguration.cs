using CNSS_TRESO.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(100);
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.HasIndex(x => x.OccurredAt);
    }
}
