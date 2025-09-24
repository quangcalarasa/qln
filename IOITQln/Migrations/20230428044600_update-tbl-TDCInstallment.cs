using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblTDCInstallment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TDCInstallmentPriceId",
                table: "TDCInstallmentTemporaryDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TDCInstallmentPriceId",
                table: "TDCInstallmentPriceAndTax",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TDCInstallmentPriceId",
                table: "TDCInstallmentOfficialDetail",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TDCInstallmentPriceId",
                table: "TDCInstallmentTemporaryDetail");

            migrationBuilder.DropColumn(
                name: "TDCInstallmentPriceId",
                table: "TDCInstallmentPriceAndTax");

            migrationBuilder.DropColumn(
                name: "TDCInstallmentPriceId",
                table: "TDCInstallmentOfficialDetail");
        }
    }
}
