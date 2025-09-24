using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblTDCInstallment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TDCInstallmentOfficialDetail",
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
                    IngredientsPriceId = table.Column<int>(nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCInstallmentOfficialDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TDCInstallmentPrice",
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
                    TdcCustomerId = table.Column<int>(nullable: false),
                    ContractNumber = table.Column<string>(maxLength: 5000, nullable: true),
                    DateNumber = table.Column<DateTime>(nullable: false),
                    NewContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DifferenceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Floor1 = table.Column<int>(nullable: false),
                    TdcProjectId = table.Column<int>(nullable: false),
                    LandId = table.Column<int>(nullable: false),
                    PlatformId = table.Column<int>(nullable: false),
                    BlockHouseId = table.Column<int>(nullable: false),
                    FloorTdcId = table.Column<int>(nullable: false),
                    TdcApartmentId = table.Column<int>(nullable: false),
                    Corner = table.Column<bool>(nullable: false),
                    TemporaryDecreeNumber = table.Column<string>(maxLength: 5000, nullable: true),
                    TemporaryDecreeDate = table.Column<DateTime>(nullable: false),
                    TemporaryTotalArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TemporaryTotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DecreeNumber = table.Column<string>(maxLength: 5000, nullable: true),
                    DecreeDate = table.Column<DateTime>(nullable: false),
                    TotalArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearPay = table.Column<int>(nullable: false),
                    FirstPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FirstPayDate = table.Column<DateTime>(nullable: false),
                    TotalPayValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCInstallmentPrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TDCInstallmentPriceAndTax",
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
                    Year = table.Column<int>(nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCInstallmentPriceAndTax", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TDCInstallmentTemporaryDetail",
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
                    IngredientsPriceId = table.Column<int>(nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCInstallmentTemporaryDetail", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TDCInstallmentOfficialDetail");

            migrationBuilder.DropTable(
                name: "TDCInstallmentPrice");

            migrationBuilder.DropTable(
                name: "TDCInstallmentPriceAndTax");

            migrationBuilder.DropTable(
                name: "TDCInstallmentTemporaryDetail");
        }
    }
}
