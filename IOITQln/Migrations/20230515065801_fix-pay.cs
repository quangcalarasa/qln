using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixpay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "TdcPriceOneSellPay");

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
                name: "MoneyCenter",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "MoneyPublic",
                table: "TdcPriceOneSellPay");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "TdcPriceOneSellPay");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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
                name: "MoneyCenter",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSellPay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPublic",
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
    }
}
