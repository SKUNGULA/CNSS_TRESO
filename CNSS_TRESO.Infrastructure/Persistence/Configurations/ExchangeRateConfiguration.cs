using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("Tab taux");
        builder.HasKey(x => x.Number);
        builder.Property(x => x.Number).HasColumnName("N°").ValueGeneratedOnAdd();
        builder.Property(x => x.RateDate).HasColumnName("Date").HasColumnType("date").IsRequired();
        builder.Property(x => x.CurrencyCode).HasColumnName("Monnaie").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("Montant taux").HasColumnType("money");
        builder.Property(x => x.Percent).HasColumnName("Pourcent");
    }
}
