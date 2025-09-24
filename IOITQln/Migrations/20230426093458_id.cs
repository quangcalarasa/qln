using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LandId",
                table: "FloorTdc",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TDCProjectId",
                table: "FloorTdc",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TDCProjectId",
                table: "BlockHouse",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandId",
                table: "ApartmentTdcs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TDCProjectId",
                table: "ApartmentTdcs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandId",
                table: "FloorTdc");

            migrationBuilder.DropColumn(
                name: "TDCProjectId",
                table: "FloorTdc");

            migrationBuilder.DropColumn(
                name: "TDCProjectId",
                table: "BlockHouse");

            migrationBuilder.DropColumn(
                name: "LandId",
                table: "ApartmentTdcs");

            migrationBuilder.DropColumn(
                name: "TDCProjectId",
                table: "ApartmentTdcs");
        }
    }
}
