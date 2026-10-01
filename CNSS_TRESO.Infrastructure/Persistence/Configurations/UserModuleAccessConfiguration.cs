using CNSS_TRESO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class UserModuleAccessConfiguration : IEntityTypeConfiguration<UserModuleAccess>
{
    public void Configure(EntityTypeBuilder<UserModuleAccess> builder)
    {
        builder.ToTable("Table accès utilisateur");
        builder.HasKey(x => new { x.Username, x.ModuleNumber });
        builder.Property(x => x.Username).HasColumnName("Utilisateurs").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ModuleNumber).HasColumnName("Formulaire").IsRequired();
    }
}
