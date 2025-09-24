using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblPricingApartmentLandDetailPricingLandTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coefficient",
                table: "PricingLandTbl");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceDifference",
                table: "TdcPriceRentDataExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CoefficientDistribution",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CoefficientUseValue",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LandUnitPrice",
                table: "PricingApartmentLandDetail",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoefficientDistribution",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "CoefficientUseValue",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "LandUnitPrice",
                table: "PricingApartmentLandDetail");

            migrationBuilder.AlterColumn<string>(
                name: "PriceDifference",
                table: "TdcPriceRentDataExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<float>(
                name: "Coefficient",
                table: "PricingLandTbl",
                type: "real",
                nullable: true);
        }
    }
}
