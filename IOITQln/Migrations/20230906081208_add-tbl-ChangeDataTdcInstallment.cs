using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblChangeDataTdcInstallment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "TDCInstallmentPrice",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TDCInstallmentPrice",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "TDCInstallmentPrice");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TDCInstallmentPrice");
        }
    }
}
