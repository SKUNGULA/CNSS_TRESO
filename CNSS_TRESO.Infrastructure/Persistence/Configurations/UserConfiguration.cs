using CNSS_TRESO.Domain.Entities;
using CNSS_TRESO.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Table utilisateurs");
        builder.HasKey(x => x.Username);

        builder.Property(x => x.Username)
            .HasColumnName("Nom utilisateur")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Matricule)
            .HasColumnName("Identifiant")
            .HasMaxLength(10);

        builder.Property(x => x.Password)
            .HasColumnName("Mot de passe")
            .HasMaxLength(20);

        builder.Property(x => x.CostCenter)
            .HasColumnName("CentreFrais")
            .HasMaxLength(50);

        builder.Property(x => x.AccessLevel)
            .HasColumnName("Niveau acces")
            .HasMaxLength(1)
            .HasConversion(
                v => ((int)v).ToString(),
                v => string.IsNullOrWhiteSpace(v) ? AccessLevel.Agent : (AccessLevel)int.Parse(v));

        builder.Ignore(x => x.Id);
        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.PasswordHash);
        builder.Ignore(x => x.IsActive);
    }
}
