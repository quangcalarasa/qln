using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_PricingLandTbl_addfieldsNCC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyInvestmentRate",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorApplyPriceChange",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMezzanine",
                table: "PricingLandTbl",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyInvestmentRate",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "FloorApplyPriceChange",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "IsMezzanine",
                table: "PricingLandTbl");
        }
    }
}
