using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class thembaocao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VAT",
                table: "TdcPriceOneSell",
                newName: "VATTT");

            migrationBuilder.RenameColumn(
                name: "MoneyPrincipal",
                table: "TdcPriceOneSell",
                newName: "VATCT");

            migrationBuilder.RenameColumn(
                name: "Mintenance",
                table: "TdcPriceOneSell",
                newName: "MoneyPrincipalTT");

            migrationBuilder.RenameColumn(
                name: "Manage3",
                table: "TdcPriceOneSell",
                newName: "MoneyPrincipalCT");

            migrationBuilder.RenameColumn(
                name: "Manage2",
                table: "TdcPriceOneSell",
                newName: "Manage3TT");

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
                name: "MintenanceCT",
                table: "TdcPriceOneSell",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MintenanceTT",
                table: "TdcPriceOneSell",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "MintenanceCT",
                table: "TdcPriceOneSell");

            migrationBuilder.DropColumn(
                name: "MintenanceTT",
                table: "TdcPriceOneSell");

            migrationBuilder.RenameColumn(
                name: "VATTT",
                table: "TdcPriceOneSell",
                newName: "VAT");

            migrationBuilder.RenameColumn(
                name: "VATCT",
                table: "TdcPriceOneSell",
                newName: "MoneyPrincipal");

            migrationBuilder.RenameColumn(
                name: "MoneyPrincipalTT",
                table: "TdcPriceOneSell",
                newName: "Mintenance");

            migrationBuilder.RenameColumn(
                name: "MoneyPrincipalCT",
                table: "TdcPriceOneSell",
                newName: "Manage3");

            migrationBuilder.RenameColumn(
                name: "Manage3TT",
                table: "TdcPriceOneSell",
                newName: "Manage2");
        }
    }
}
