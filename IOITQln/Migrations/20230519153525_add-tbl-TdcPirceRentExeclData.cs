using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblTdcPirceRentExeclData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TdcPriceRentExcel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    PaymentTimes = table.Column<string>(nullable: true),
                    PaymentDatePrescribed = table.Column<string>(nullable: true),
                    PaymentDatePrescribed1 = table.Column<string>(nullable: true),
                    ExpectedPaymentDate = table.Column<string>(nullable: true),
                    DailyInterest = table.Column<string>(nullable: true),
                    DailyInterestRate = table.Column<string>(nullable: true),
                    UnitPay = table.Column<string>(nullable: true),
                    PriceEarnings = table.Column<string>(nullable: true),
                    PricePaymentPeriod = table.Column<string>(nullable: true),
                    Pay = table.Column<string>(nullable: true),
                    Paid = table.Column<string>(nullable: true),
                    PriceDifference = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceRentExcel", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcPriceRentExcel");
        }
    }
}
