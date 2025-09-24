using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class phieuchi4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Md167HouseId",
                table: "Md167ManagePayments");

            migrationBuilder.DropColumn(
                name: "TypeHouse",
                table: "HousePayments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Md167HouseId",
                table: "Md167ManagePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeHouse",
                table: "HousePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
