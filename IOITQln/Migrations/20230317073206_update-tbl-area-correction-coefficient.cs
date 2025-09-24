using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblareacorrectioncoefficient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameOfConstruction",
                table: "AreaCorrectionCoefficient");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AreaCorrectionCoefficient",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AreaCorrectionCoefficient");

            migrationBuilder.AddColumn<string>(
                name: "NameOfConstruction",
                table: "AreaCorrectionCoefficient",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
