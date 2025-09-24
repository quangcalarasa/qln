using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class ađtblupdateRentingPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DecreeId",
                table: "RentingPrice",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitPriceId",
                table: "RentingPrice",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeId",
                table: "RentingPrice");

            migrationBuilder.DropColumn(
                name: "UnitPriceId",
                table: "RentingPrice");
        }
    }
}
