using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblupdarteTdcPriceRentExcel2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyInterest",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "DailyInterestRate",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "ExpectedPaymentDate",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "Pay",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "PaymentDatePrescribed",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "PaymentDatePrescribed1",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "PaymentTimes",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "PriceDifference",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "PriceEarnings",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "PricePaymentPeriod",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "UnitPay",
                table: "TdcPriceRent");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPay",
                table: "TdcPriceRentExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePaymentPeriod",
                table: "TdcPriceRentExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceEarnings",
                table: "TdcPriceRentExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Pay",
                table: "TdcPriceRentExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Paid",
                table: "TdcPriceRentExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyInterestRate",
                table: "TdcPriceRentExcel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DailyInterest",
                table: "TdcPriceRentExcel",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TdcPriceRentId",
                table: "TdcPriceRentExcel",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcPriceRentId",
                table: "TdcPriceRentExcel");

            migrationBuilder.AlterColumn<string>(
                name: "UnitPay",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PricePaymentPeriod",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PriceEarnings",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Pay",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Paid",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "DailyInterestRate",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "DailyInterest",
                table: "TdcPriceRentExcel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "DailyInterest",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DailyInterestRate",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedPaymentDate",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Paid",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pay",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDatePrescribed",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDatePrescribed1",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTimes",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceDifference",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceEarnings",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricePaymentPeriod",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitPay",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
