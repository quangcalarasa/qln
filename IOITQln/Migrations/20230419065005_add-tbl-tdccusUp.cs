using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbltdccusUp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "TdcAuthCustomerDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lane",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Lane",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "TdcCustomer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "TdcAuthCustomerDetail",
                type: "int",
                nullable: true);
        }
    }
}
