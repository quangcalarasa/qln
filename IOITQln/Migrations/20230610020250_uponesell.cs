using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class uponesell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Manage2CT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "Manage2TT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "Manage3CT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "Manage3TT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "MintenanceCT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "MintenanceTT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "MoneyPrincipalCT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "MoneyPrincipalTT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "VATCT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "VATTT",
                table: "TdcPriceOneSell");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Manage2CT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage2TT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage3CT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Manage3TT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MintenanceCT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MintenanceTT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPrincipalCT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MoneyPrincipalTT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATCT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATTT",
                table: "TdcPriceOneSell",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
