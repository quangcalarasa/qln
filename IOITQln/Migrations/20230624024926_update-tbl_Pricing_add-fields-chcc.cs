using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Pricing_addfieldschcc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaCorrectionCoefficientId",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AreaCorrectionCoefficientValue",
                table: "Pricing",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaCorrectionCoefficientId",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "AreaCorrectionCoefficientValue",
                table: "Pricing");
        }
    }
}
