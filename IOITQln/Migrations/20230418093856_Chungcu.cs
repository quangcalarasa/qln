using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class Chungcu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "ResettlementApartments");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ResettlementApartments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ResettlementApartments");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ResettlementApartments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
