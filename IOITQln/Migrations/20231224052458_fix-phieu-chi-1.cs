using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixphieuchi1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Debt",
                table: "HousePayments",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Paid",
                table: "HousePayments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Debt",
                table: "HousePayments");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "HousePayments");
        }
    }
}
