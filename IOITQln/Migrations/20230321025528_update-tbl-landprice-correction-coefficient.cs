using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbllandpricecorrectioncoefficient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlleyWidth",
                table: "LandCorrectionCoefficient");

            migrationBuilder.AddColumn<float>(
                name: "FacadeWidth",
                table: "LandCorrectionCoefficient",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacadeWidth",
                table: "LandCorrectionCoefficient");

            migrationBuilder.AddColumn<float>(
                name: "AlleyWidth",
                table: "LandCorrectionCoefficient",
                type: "real",
                nullable: true);
        }
    }
}
