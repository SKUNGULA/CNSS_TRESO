using System;
using CNSS_TRESO.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CNSS_TRESO.Infrastructure.Migrations;

[DbContext(typeof(CNSS_TRESO.Infrastructure.Persistence.CnssTresoDbContext))]
public partial class CnssTresoDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("CNSS_TRESO.Domain.Entities.User", b =>
        {
            b.Property<Guid>("Id");
            b.Property<int>("AccessLevel");
            b.Property<string>("CostCenter").HasMaxLength(50);
            b.Property<bool>("IsActive");
            b.Property<string>("FullName").HasMaxLength(200);
            b.Property<string>("PasswordHash").HasMaxLength(500);
            b.Property<string>("Username").HasMaxLength(100);
            b.HasKey("Id");
            b.HasIndex("Username").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity("CNSS_TRESO.Domain.Treasury.ExchangeRate", b =>
        {
            b.Property<Guid>("Id");
            b.Property<decimal>("Amount").HasPrecision(18, 2);
            b.Property<string>("CurrencyCode").HasMaxLength(10);
            b.Property<DateOnly>("RateDate").HasColumnType("date");
            b.HasKey("Id");
            b.ToTable("ExchangeRates");
        });

        modelBuilder.Entity("CNSS_TRESO.Domain.Treasury.TreasuryMovement", b =>
        {
            b.Property<Guid>("Id");
            b.Property<int>("CreatedByLevel");
            b.Property<string>("CostCenter").HasMaxLength(50);
            b.Property<decimal>("Amount").HasPrecision(18, 2);
            b.Property<string>("CurrencyCode").HasMaxLength(10);
            b.Property<string>("Description").HasMaxLength(500);
            b.Property<string>("JournalCode").HasMaxLength(50);
            b.Property<DateOnly>("MovementDate").HasColumnType("date");
            b.Property<string>("ReferenceNumber").HasMaxLength(100);
            b.Property<int>("Status");
            b.HasKey("Id");
            b.ToTable("TreasuryMovements");
        });
    }
}
