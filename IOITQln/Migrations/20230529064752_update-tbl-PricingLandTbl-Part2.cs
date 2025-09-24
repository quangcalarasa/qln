using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblPricingLandTblPart2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralAreaValue",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "PersonalAreaValue",
                table: "PricingLandTbl");

            migrationBuilder.AddColumn<float>(
                name: "GeneralArea",
                table: "PricingLandTbl",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PrivateArea",
                table: "PricingLandTbl",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralArea",
                table: "PricingLandTbl");

            migrationBuilder.DropColumn(
                name: "PrivateArea",
                table: "PricingLandTbl");

            migrationBuilder.AddColumn<float>(
                name: "GeneralAreaValue",
                table: "PricingLandTbl",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PersonalAreaValue",
                table: "PricingLandTbl",
                type: "real",
                nullable: true);
        }
    }
}
