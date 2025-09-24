using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblpricing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pricing",
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
                    BlockId = table.Column<int>(nullable: false),
                    ApartmentId = table.Column<int>(nullable: false),
                    DateCreate = table.Column<DateTime>(nullable: true),
                    TimeUse = table.Column<string>(maxLength: 4000, nullable: true),
                    VatId = table.Column<int>(nullable: true),
                    Vat = table.Column<float>(nullable: true),
                    ApartmentPrice = table.Column<decimal>(nullable: true),
                    ApartmentPriceReduced = table.Column<decimal>(nullable: true),
                    ApartmentPriceRemaining = table.Column<decimal>(nullable: true),
                    ApartmentPriceNoVat = table.Column<decimal>(nullable: true),
                    ApartmentPriceVat = table.Column<decimal>(nullable: true),
                    ApartmentPriceReducedNote = table.Column<decimal>(nullable: true),
                    LandPrice = table.Column<decimal>(nullable: true),
                    DeductionLandMoneyId = table.Column<int>(nullable: true),
                    DeductionLandMoneyValue = table.Column<float>(nullable: true),
                    ConversionArea = table.Column<float>(nullable: true),
                    LandPriceAfterReduced = table.Column<decimal>(nullable: true),
                    TotalPrice = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingConstructionPrice",
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
                    PricingId = table.Column<int>(nullable: false),
                    ConstructionPriceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingConstructionPrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingCustomer",
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
                    PricingId = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingLandTbl",
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
                    PricingId = table.Column<int>(nullable: false),
                    ApartmentId = table.Column<int>(nullable: false),
                    LevelApartment = table.Column<int>(nullable: false),
                    FloorApplyCoefficient = table.Column<int>(nullable: false),
                    FloorId = table.Column<int>(nullable: false),
                    AreaId = table.Column<int>(nullable: false),
                    GeneralAreaValue = table.Column<float>(nullable: true),
                    PersonalAreaValue = table.Column<float>(nullable: true),
                    Coefficient = table.Column<float>(nullable: true),
                    MaintextureRateValue = table.Column<float>(nullable: true),
                    PriceListItemId = table.Column<int>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    PriceInYear = table.Column<decimal>(nullable: true),
                    RemainingPrice = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingLandTbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingOfficer",
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
                    PricingId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Function = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingOfficer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingReducedPerson",
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
                    CustomerId = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: true),
                    Salary = table.Column<decimal>(nullable: true),
                    Value = table.Column<float>(nullable: true),
                    DeductionCoefficient = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingReducedPerson", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pricing");

            migrationBuilder.DropTable(
                name: "PricingConstructionPrice");

            migrationBuilder.DropTable(
                name: "PricingCustomer");

            migrationBuilder.DropTable(
                name: "PricingLandTbl");

            migrationBuilder.DropTable(
                name: "PricingOfficer");

            migrationBuilder.DropTable(
                name: "PricingReducedPerson");
        }
    }
}
