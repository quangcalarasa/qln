using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class _1pay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MoneyCenter",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPublic",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoneyCenter",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "MoneyPublic",
                table: "TdcPriceOneSellPay");
        }
    }
}
