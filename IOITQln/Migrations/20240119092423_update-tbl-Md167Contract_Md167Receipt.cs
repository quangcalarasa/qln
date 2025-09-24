using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblMd167Contract_Md167Receipt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PaidDeposit",
                table: "Md167Receipt",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaidDeposit",
                table: "Md167Contract",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RefundPaidDeposit",
                table: "Md167Contract",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidDeposit",
                table: "Md167Receipt");

            migrationBuilder.DropColumn(
                name: "PaidDeposit",
                table: "Md167Contract");

            migrationBuilder.DropColumn(
                name: "RefundPaidDeposit",
                table: "Md167Contract");
        }
    }
}
