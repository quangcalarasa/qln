using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblTdcPriceRentEtt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcBlockHouseId",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "TdcFloorTdcId",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "TdcLandId",
                table: "TdcPriceRent");

            migrationBuilder.AddColumn<int>(
                name: "BlockHouseId",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FloorTdcId",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LandId",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockHouseId",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "FloorTdcId",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "LandId",
                table: "TdcPriceRent");

            migrationBuilder.AddColumn<int>(
                name: "TdcBlockHouseId",
                table: "TdcPriceRent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TdcFloorTdcId",
                table: "TdcPriceRent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TdcLandId",
                table: "TdcPriceRent",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
