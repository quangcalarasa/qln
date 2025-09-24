using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class tienban1lan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Manage2",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage3",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Mintenance",
                table: "TdcPriceOneSell",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VAT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Manage2",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "Manage3",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "Mintenance",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "TdcPriceOneSell");
        }
    }
}
