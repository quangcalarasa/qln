using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class soluong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlockHouseCount",
                table: "Land",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentTdcCount",
                table: "FloorTdc",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentTdcCount",
                table: "BlockHouse",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FloorTdcCount",
                table: "BlockHouse",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockHouseCount",
                table: "Land");

            migrationBuilder.DropColumn(
                name: "ApartmentTdcCount",
                table: "FloorTdc");

            migrationBuilder.DropColumn(
                name: "ApartmentTdcCount",
                table: "BlockHouse");

            migrationBuilder.DropColumn(
                name: "FloorTdcCount",
                table: "BlockHouse");
        }
    }
}
