using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class thanhtoan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Manage2",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage3",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Mintenance",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VAT",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Manage2",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "Manage3",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "Mintenance",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "TdcPriceOneSellPay");
        }
    }
}
