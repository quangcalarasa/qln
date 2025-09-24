using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblinstallmentpricepaytdc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstallmentPriceTableTdc",
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
                    RowStatus = table.Column<int>(nullable: true),
                    PayTimeId = table.Column<int>(nullable: true),
                    TypeRow = table.Column<byte>(nullable: true),
                    StatusTdcTable = table.Column<int>(nullable: false),
                    PaymentTimes = table.Column<int>(nullable: true),
                    PayDateDefault = table.Column<DateTime>(nullable: true),
                    PayDateBefore = table.Column<DateTime>(nullable: true),
                    PayDateGuess = table.Column<DateTime>(nullable: true),
                    PayDateReal = table.Column<DateTime>(nullable: true),
                    MonthInterest = table.Column<int>(nullable: true),
                    DailyInterest = table.Column<int>(nullable: true),
                    MonthInterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DailyInterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalPay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PayAnnual = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalPayAnnual = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Pay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Paid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentPriceTableTdc", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstallmentPriceTableTdc");
        }
    }
}
