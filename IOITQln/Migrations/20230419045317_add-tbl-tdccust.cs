using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbltdccust : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaName",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Lane",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "AreaName",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "Lane",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "TdcAuthCustomerDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.AddColumn<string>(
                name: "AreaName",
                table: "TdcCustomer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "TdcCustomer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaName",
                table: "TdcAuthCustomerDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "TdcAuthCustomerDetail",
                type: "int",
                nullable: true);
        }
    }
}
