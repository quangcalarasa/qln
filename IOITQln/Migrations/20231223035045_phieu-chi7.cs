using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class phieuchi7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                table: "HousePayments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HouseName",
                table: "HousePayments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LaneName",
                table: "HousePayments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceName",
                table: "HousePayments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WardName",
                table: "HousePayments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictName",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "HouseName",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "LaneName",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "ProvinceName",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "WardName",
                table: "HousePayments");

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "HousePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "HousePayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaneId",
                table: "HousePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "HousePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardId",
                table: "HousePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
