using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addchangeRentFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeHS",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileStatus",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeHS",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "FileStatus",
                table: "RentFile");
        }
    }
}
