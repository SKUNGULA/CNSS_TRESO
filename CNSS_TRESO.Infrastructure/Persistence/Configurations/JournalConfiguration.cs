using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.ToTable("Table identification journaux");
        builder.HasKey(x => x.Number);

        builder.Property(x => x.Number).HasColumnName("Numéro journal").HasMaxLength(50);
        builder.Property(x => x.Label).HasColumnName("Libellé journal").HasMaxLength(100);
        builder.Property(x => x.Type).HasColumnName("Type journal").HasMaxLength(1);
        builder.Property(x => x.Account).HasColumnName("N° Compte").HasMaxLength(15);
        builder.Property(x => x.CostCenter).HasColumnName("Centre de frais").HasMaxLength(4);
        builder.Property(x => x.Currency).HasColumnName("Monnaie").HasMaxLength(10);
        builder.Property(x => x.NextPiece).HasColumnName("Numéro pièce");
        builder.Property(x => x.MovementType).HasColumnName("Type Mouvement").HasMaxLength(1);
    }
}

public sealed class SignatoryConfiguration : IEntityTypeConfiguration<Signatory>
{
    public void Configure(EntityTypeBuilder<Signatory> builder)
    {
        builder.ToTable("Table signateur");
        builder.HasKey(x => x.Journal);

        builder.Property(x => x.Journal).HasColumnName("journal").HasMaxLength(50);
        builder.Property(x => x.LeftName).HasColumnName("Signature de gauche");
        builder.Property(x => x.LeftTitle).HasColumnName("Titre de gauche");
        builder.Property(x => x.RightName).HasColumnName("Signature de droite");
        builder.Property(x => x.RightTitle).HasColumnName("Titre de droite");
        builder.Property(x => x.CenterName).HasColumnName("Signature de centre");
        builder.Property(x => x.CenterTitle).HasColumnName("Titre de centre");
        builder.Property(x => x.OfficerName).HasColumnName("Signature du prepose");
        builder.Property(x => x.OfficerTitle).HasColumnName("Titre du prepose");
        builder.Property(x => x.AccountantName).HasColumnName("Signature du comptable");
        builder.Property(x => x.AccountantTitle).HasColumnName("Titre du comptable");
        builder.Property(x => x.CostCenter).HasColumnName("CentreFrais");
    }
}
