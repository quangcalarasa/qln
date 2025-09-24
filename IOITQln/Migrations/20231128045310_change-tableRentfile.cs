using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class changetableRentfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Art",
                table: "RentFile",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IndexC",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IndexH",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SHNN",
                table: "RentFile",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
