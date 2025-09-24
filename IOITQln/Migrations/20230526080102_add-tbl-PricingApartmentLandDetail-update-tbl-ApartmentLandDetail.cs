using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblPricingApartmentLandDetailupdatetblApartmentLandDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralUseAreaValue",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "PersonalUseAreaValue",
                table: "ApartmentLandDetail");

            migrationBuilder.AddColumn<float>(
                name: "GeneralArea",
                table: "ApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PrivateArea",
                table: "ApartmentLandDetail",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PricingApartmentLandDetail",
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
                    ApartmentLandDetailId = table.Column<long>(nullable: false),
                    ApartmentId = table.Column<int>(nullable: false),
                    DecreeType1Id = table.Column<int>(nullable: true),
                    TermApply = table.Column<int>(nullable: true),
                    GeneralArea = table.Column<float>(nullable: true),
                    PrivateArea = table.Column<float>(nullable: true),
                    LandPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeductionLandMoneyId = table.Column<int>(nullable: true),
                    DeductionLandMoneyValue = table.Column<float>(nullable: true),
                    ConversionArea = table.Column<float>(nullable: true),
                    LandPriceAfterReduced = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingApartmentLandDetail", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "GeneralArea",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "PrivateArea",
                table: "ApartmentLandDetail");

            migrationBuilder.AddColumn<float>(
                name: "GeneralUseAreaValue",
                table: "ApartmentLandDetail",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PersonalUseAreaValue",
                table: "ApartmentLandDetail",
                type: "real",
                nullable: true);
        }
    }
}
