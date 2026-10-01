using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Table documents");
        builder.HasKey(x => x.Num);
        builder.Property(x => x.Num).HasColumnName("num").ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasColumnName("Code document").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Label).HasColumnName("Libellé document");
        builder.Property(x => x.Journal).HasColumnName("Journal").HasMaxLength(10);
        builder.Property(x => x.Number).HasColumnName("Numéro document");
        builder.Property(x => x.MovementType).HasColumnName("Type mouvement").HasMaxLength(3);
        builder.Property(x => x.Direction).HasColumnName("Sens mouvement").HasMaxLength(1);
        builder.Property(x => x.JournalType).HasColumnName("Type journal").HasMaxLength(1);
        builder.Property(x => x.CurrencyCode).HasColumnName("Monnaie").HasMaxLength(6);
        builder.Property(x => x.BedRequired).HasColumnName("BED obligatoire").HasMaxLength(10);
    }
}
