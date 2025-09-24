using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbltabletdcinstallexcel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MonthInterestRate",
                table: "InstallmentPriceTableTdc",
                type: "decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyInterestRate",
                table: "InstallmentPriceTableTdc",
                type: "decimal(18,5)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MonthInterestRate",
                table: "InstallmentPriceTableTdc",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,5)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyInterestRate",
                table: "InstallmentPriceTableTdc",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,5)",
                oldNullable: true);
        }
    }
}
