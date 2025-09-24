using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbltdc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChangeTimes",
                table: "TdcPriceRentOfficial",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChangeTimes",
                table: "TdcPriceOneSellOfficial",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChangeTimes",
                table: "TDCInstallmentOfficialDetail",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeTimes",
                table: "TdcPriceRentOfficial");

            migrationBuilder.DropColumn(
                name: "ChangeTimes",
                table: "TdcPriceOneSellOfficial");

            migrationBuilder.DropColumn(
                name: "ChangeTimes",
                table: "TDCInstallmentOfficialDetail");
        }
    }
}
