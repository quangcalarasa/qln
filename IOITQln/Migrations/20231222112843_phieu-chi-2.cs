using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class phieuchi2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "HousePayments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "HousePayments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaneId",
                table: "HousePayments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "HousePayments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardId",
                table: "HousePayments",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "LaneId",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "HousePayments");
        }
    }
}
