using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class TreasuryMovementConfiguration : IEntityTypeConfiguration<TreasuryMovement>
{
    public void Configure(EntityTypeBuilder<TreasuryMovement> builder)
    {
        builder.ToTable("Table mouvements");
        builder.HasKey(x => x.Number);
        builder.Property(x => x.Number).HasColumnName("N°").ValueGeneratedOnAdd();
        builder.Property(x => x.Journal).HasColumnName("Journal").HasMaxLength(100).IsRequired();
        builder.Property(x => x.PieceNumber).HasColumnName("Numéro pièce");
        builder.Property(x => x.DocumentCode).HasColumnName("Document").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DocumentNumber).HasColumnName("Numéro document").IsRequired();
        builder.Property(x => x.MovementDate).HasColumnName("Date mouvement").HasColumnType("date");
        builder.Property(x => x.Description).HasColumnName("Libellé mouvement");
        builder.Property(x => x.Amount).HasColumnName("Montant").HasColumnType("money");
        builder.Property(x => x.Rate).HasColumnName("Taux").HasColumnType("money");
        builder.Property(x => x.MovementType).HasColumnName("Type mouvement").HasMaxLength(3);
        builder.Property(x => x.Direction).HasColumnName("Sens mouvement").HasMaxLength(1);
        builder.Property(x => x.CurrencyCode).HasColumnName("Monnaie").HasMaxLength(4);
        builder.Property(x => x.Account).HasColumnName("Compte").HasMaxLength(15);
        builder.Property(x => x.AccountType).HasColumnName("Type compte").HasMaxLength(2);
        builder.Property(x => x.CostCenter).HasColumnName("cf").HasMaxLength(50);
        builder.Property(x => x.Payer).HasColumnName("Payeur");
        builder.Property(x => x.Beneficiary).HasColumnName("Bénéficiaire");
        builder.Property(x => x.CheckNumber).HasColumnName("Numéro chèque").HasMaxLength(50);
        builder.Property(x => x.Bank).HasColumnName("Banque").HasMaxLength(100);
        builder.Property(x => x.Drawer).HasColumnName("Tireur").HasMaxLength(50);
        builder.Property(x => x.CheckDate).HasColumnName("Date chèque").HasMaxLength(50);
        builder.Property(x => x.Validated).HasColumnName("Valider");
        builder.Property(x => x.Accounted).HasColumnName("Comptabilisé");
        builder.Property(x => x.UserName).HasColumnName("User").HasMaxLength(50);
        builder.Property(x => x.Society).HasColumnName("Société").HasMaxLength(2);
        builder.Property(x => x.Period).HasColumnName("Période");
        builder.Property(x => x.EntryDate).HasColumnName("Date saisie").HasMaxLength(50);
    }
}
