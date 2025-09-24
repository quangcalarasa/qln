using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblupdateDataBaseTdcPriceRent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyInterest",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DailyInterestRate",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedPaymentDate",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "TdcPriceRent",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Paid",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Pay",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDatePrescribed",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDatePrescribed1",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PaymentTimes",
                table: "TdcPriceRent",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceDifference",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceEarnings",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePaymentPeriod",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPay",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
