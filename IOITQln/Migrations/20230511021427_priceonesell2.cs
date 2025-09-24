using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class priceonesell2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcPriceOneTimeId",
                table: "TdcPriceOneSellTax");

            migrationBuilder.AddColumn<int>(
                name: "TdcPriceOneSellId",
                table: "TdcPriceOneSellTax",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcPriceOneSellId",
                table: "TdcPriceOneSellTax");

            migrationBuilder.AddColumn<int>(
                name: "TdcPriceOneTimeId",
                table: "TdcPriceOneSellTax",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
