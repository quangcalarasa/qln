using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbls_Md167Contract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Md167AuctionDecision",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Md167ContractId = table.Column<int>(nullable: false),
                    Decision = table.Column<string>(maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateEffect = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167AuctionDecision", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Contract",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Code = table.Column<string>(maxLength: 500, nullable: false),
                    DateSign = table.Column<DateTime>(nullable: false),
                    DelegateId = table.Column<int>(nullable: false),
                    HouseId = table.Column<int>(nullable: false),
                    TypePrice = table.Column<int>(nullable: false),
                    RentalPeriod = table.Column<int>(nullable: false),
                    NoteRentalPeriod = table.Column<string>(maxLength: 2000, nullable: true),
                    RentalPurpose = table.Column<int>(nullable: false),
                    NoteRentalPurpose = table.Column<string>(maxLength: 2000, nullable: true),
                    PaymentPeriod = table.Column<int>(nullable: false),
                    DateGroundHandover = table.Column<DateTime>(nullable: false),
                    ContractStatus = table.Column<int>(nullable: false),
                    ContractExtension = table.Column<string>(maxLength: 2000, nullable: true),
                    Liquidation = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Contract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167PricePerMonth",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Md167ContractId = table.Column<int>(nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HousePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VatPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167PricePerMonth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Valuation",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Md167ContractId = table.Column<int>(nullable: false),
                    UnitId = table.Column<int>(nullable: false),
                    Attactment = table.Column<string>(maxLength: 2000, nullable: true),
                    DateEffect = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Valuation", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Md167AuctionDecision");

            migrationBuilder.DropTable(
                name: "Md167Contract");

            migrationBuilder.DropTable(
                name: "Md167PricePerMonth");

            migrationBuilder.DropTable(
                name: "Md167Valuation");
        }
    }
}
