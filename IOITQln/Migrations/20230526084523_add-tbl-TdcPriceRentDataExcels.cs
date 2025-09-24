using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblTdcPriceRentDataExcels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "TdcPriceRentDataExcel",
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
                    CountYear = table.Column<int>(nullable: false),
                    index = table.Column<int>(nullable: false),
                    TdcPriceRentId = table.Column<int>(nullable: false),
                    PaymentTimes = table.Column<string>(nullable: true),
                    PaymentDatePrescribed = table.Column<DateTime>(nullable: false),
                    PaymentDatePrescribed1 = table.Column<DateTime>(nullable: false),
                    ExpectedPaymentDate = table.Column<DateTime>(nullable: false),
                    DailyInterest = table.Column<int>(nullable: false),
                    DailyInterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePaymentPeriod = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Pay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceRentDataExcel", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcPriceRentDataExcel");

           
        }
    }
}
