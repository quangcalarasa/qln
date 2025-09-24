using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addentitiesTBLRentfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Art",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "IndexC",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "IndexH",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "SHNN",
                table: "RentFile");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaneId",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "proviceId",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "LaneId",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "proviceId",
                table: "RentFile");

            migrationBuilder.AddColumn<bool>(
                name: "Art",
                table: "RentFile",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IndexC",
                table: "RentFile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IndexH",
                table: "RentFile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SHNN",
                table: "RentFile",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
