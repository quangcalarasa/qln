using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblchangeDataBasaRentingPrice2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentLevel",
                table: "RentingPrice");

            migrationBuilder.DropColumn(
                name: "ApartmentType",
                table: "RentingPrice");

            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "RentingPrice",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeBlockId",
                table: "RentingPrice",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "RentingPrice");

            migrationBuilder.DropColumn(
                name: "TypeBlockId",
                table: "RentingPrice");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentLevel",
                table: "RentingPrice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentType",
                table: "RentingPrice",
                type: "int",
                nullable: true);
        }
    }
}
