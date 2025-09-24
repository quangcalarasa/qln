using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class khoinha1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentTdcCount",
                table: "BlockHouse");

            migrationBuilder.AddColumn<int>(
                name: "TotalApartmentTdcCount",
                table: "BlockHouse",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalApartmentTdcCount",
                table: "BlockHouse");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentTdcCount",
                table: "BlockHouse",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
