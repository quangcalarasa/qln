using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbltdccus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLT",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "AddressLT",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.AddColumn<string>(
                name: "AddressLh",
                table: "TdcCustomer",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLH",
                table: "TdcAuthCustomerDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLh",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "AddressLH",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.AddColumn<string>(
                name: "AddressLT",
                table: "TdcCustomer",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLT",
                table: "TdcAuthCustomerDetail",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
