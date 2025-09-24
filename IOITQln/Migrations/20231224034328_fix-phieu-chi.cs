using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixphieuchi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProvinceName",
                table: "HousePayments");

            migrationBuilder.AddColumn<string>(
                name: "ProviceName",
                table: "HousePayments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviceName",
                table: "HousePayments");

            migrationBuilder.AddColumn<string>(
                name: "ProvinceName",
                table: "HousePayments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
