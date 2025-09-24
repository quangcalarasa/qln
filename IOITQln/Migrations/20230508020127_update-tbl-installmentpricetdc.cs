using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblinstallmentpricetdc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PesonalTax",
                table: "TDCInstallmentPrice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RegistrationTax",
                table: "TDCInstallmentPrice",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PesonalTax",
                table: "TDCInstallmentPrice");

            migrationBuilder.DropColumn(
                name: "RegistrationTax",
                table: "TDCInstallmentPrice");
        }
    }
}
