using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblPricingConstructionPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Value",
                table: "PricingConstructionPrice",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "PricingConstructionPrice",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "PricingConstructionPrice");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "PricingConstructionPrice");
        }
    }
}
