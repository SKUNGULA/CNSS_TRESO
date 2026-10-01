using CNSS_TRESO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CNSS_TRESO.Infrastructure.Persistence.Configurations;

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Table Module");
        builder.HasKey(x => x.Number);
        builder.Property(x => x.Number).HasColumnName("Numéro formulaire").ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnName("Nom formulaire").HasMaxLength(100);
    }
}
