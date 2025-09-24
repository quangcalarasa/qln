using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblPricingLandTbl_add_field_related_InvestmentRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InvestmentRateValue",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InvestmentRateValue2",
                table: "PricingLandTbl",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvestmentRateValue",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "InvestmentRateValue2",
                table: "PricingLandTbl");
        }
    }
}
