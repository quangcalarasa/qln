using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblPricingLandTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelApartment",
                table: "PricingLandTbl");

            migrationBuilder.AddColumn<int>(
                name: "DecreeType1Id",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermApply",
                table: "PricingLandTbl",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeType1Id",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "TermApply",
                table: "PricingLandTbl");

            migrationBuilder.AddColumn<int>(
                name: "LevelApartment",
                table: "PricingLandTbl",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
