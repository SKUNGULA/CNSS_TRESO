using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CNSS_TRESO.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExchangeRates",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                RateDate = table.Column<DateOnly>(type: "date", nullable: false),
                CurrencyCode = table.Column<string>(maxLength: 10, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExchangeRates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TreasuryMovements",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                JournalCode = table.Column<string>(maxLength: 50, nullable: false),
                MovementDate = table.Column<DateOnly>(type: "date", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CurrencyCode = table.Column<string>(maxLength: 10, nullable: false),
                ReferenceNumber = table.Column<string>(maxLength: 100, nullable: false),
                Description = table.Column<string>(maxLength: 500, nullable: false),
                Status = table.Column<int>(nullable: false),
                CreatedByLevel = table.Column<int>(nullable: false),
                CostCenter = table.Column<string>(maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TreasuryMovements", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Username = table.Column<string>(maxLength: 100, nullable: false),
                FullName = table.Column<string>(maxLength: 200, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 500, nullable: false),
                AccessLevel = table.Column<int>(nullable: false),
                CostCenter = table.Column<string>(maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ExchangeRates");
        migrationBuilder.DropTable(name: "TreasuryMovements");
        migrationBuilder.DropTable(name: "Users");
    }
}
