using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblchangeDataBasaRentingPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeId",
                table: "RentingPrice");

            migrationBuilder.AddColumn<int>(
                name: "TypeQD",
                table: "RentingPrice",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeQD",
                table: "RentingPrice");

            migrationBuilder.AddColumn<int>(
                name: "DecreeId",
                table: "RentingPrice",
                type: "int",
                nullable: true);
        }
    }
}
