using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Table monnaie");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasColumnName("Monnaie").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Label).HasColumnName("Libellé monnaie").HasMaxLength(60);
        builder.Property(x => x.Symbol).HasColumnName("Symbole").HasMaxLength(2);
    }
}
