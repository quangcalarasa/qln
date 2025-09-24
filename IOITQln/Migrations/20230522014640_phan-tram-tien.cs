using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class phantramtien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Manage2",
                table: "TdcPriceOneSellTemporary",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage3",
                table: "TdcPriceOneSellTemporary",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Mintenance",
                table: "TdcPriceOneSellTemporary",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSellTemporary",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VAT",
                table: "TdcPriceOneSellTemporary",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage2",
                table: "TdcPriceOneSellOfficial",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage3",
                table: "TdcPriceOneSellOfficial",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Mintenance",
                table: "TdcPriceOneSellOfficial",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSellOfficial",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VAT",
                table: "TdcPriceOneSellOfficial",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentCenter",
                table: "TdcPriceOneSell",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentPublic",
                table: "TdcPriceOneSell",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Manage2",
                table: "TdcPriceOneSellTemporary");

            migrationBuilder.DropColumn(
                name: "Manage3",
                table: "TdcPriceOneSellTemporary");

            migrationBuilder.DropColumn(
                name: "Mintenance",
                table: "TdcPriceOneSellTemporary");

            migrationBuilder.DropColumn(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSellTemporary");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "TdcPriceOneSellTemporary");

            migrationBuilder.DropColumn(
                name: "Manage2",
                table: "TdcPriceOneSellOfficial");

            migrationBuilder.DropColumn(
                name: "Manage3",
                table: "TdcPriceOneSellOfficial");

            migrationBuilder.DropColumn(
                name: "Mintenance",
                table: "TdcPriceOneSellOfficial");

            migrationBuilder.DropColumn(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSellOfficial");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "TdcPriceOneSellOfficial");

            migrationBuilder.DropColumn(
                name: "PaymentCenter",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "PaymentPublic",
                table: "TdcPriceOneSell");
        }
    }
}
