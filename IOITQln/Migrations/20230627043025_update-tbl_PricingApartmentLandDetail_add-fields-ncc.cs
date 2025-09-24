using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_PricingApartmentLandDetail_addfieldsncc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CoefficientDistribution",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorApplyPriceChange",
                table: "PricingApartmentLandDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoefficientDistribution",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "FloorApplyPriceChange",
                table: "PricingApartmentLandDetail");
        }
    }
}
