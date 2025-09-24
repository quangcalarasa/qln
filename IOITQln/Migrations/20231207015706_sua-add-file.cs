using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class suaaddfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Storage",
                table: "NocBlockFiles");

            migrationBuilder.DropColumn(
                name: "Storage",
                table: "NocApartmentFiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Storage",
                table: "NocBlockFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Storage",
                table: "NocApartmentFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
