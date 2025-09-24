using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblcustomerTdcUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "District",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Lane",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "District",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "Lane",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.RenameColumn(
                name: "AddressLh",
                table: "TdcCustomer",
                newName: "AddressLH");

            migrationBuilder.RenameColumn(
                name: "AddressTT",
                table: "TdcAuthCustomerDetail",
                newName: "AddressTt");

            migrationBuilder.RenameColumn(
                name: "AddressLH",
                table: "TdcAuthCustomerDetail",
                newName: "AddressLh");

            migrationBuilder.AddColumn<int>(
                name: "DistrictLH",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictTT",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneLH",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneTT",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceLH",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceTT",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardLH",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardTT",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictLh",
                table: "TdcAuthCustomerDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictTt",
                table: "TdcAuthCustomerDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneLh",
                table: "TdcAuthCustomerDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaneTt",
                table: "TdcAuthCustomerDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceLh",
                table: "TdcAuthCustomerDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceTt",
                table: "TdcAuthCustomerDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardLh",
                table: "TdcAuthCustomerDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardTt",
                table: "TdcAuthCustomerDetail",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictLH",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "DistrictTT",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "LaneLH",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "LaneTT",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "ProvinceLH",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "ProvinceTT",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "WardLH",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "WardTT",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "DistrictLh",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "DistrictTt",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "LaneLh",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "LaneTt",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "ProvinceLh",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "ProvinceTt",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "WardLh",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.DropColumn(
                name: "WardTt",
                table: "TdcAuthCustomerDetail");

            migrationBuilder.RenameColumn(
                name: "AddressLH",
                table: "TdcCustomer",
                newName: "AddressLh");

            migrationBuilder.RenameColumn(
                name: "AddressTt",
                table: "TdcAuthCustomerDetail",
                newName: "AddressTT");

            migrationBuilder.RenameColumn(
                name: "AddressLh",
                table: "TdcAuthCustomerDetail",
                newName: "AddressLH");

            migrationBuilder.AddColumn<int>(
                name: "District",
                table: "TdcCustomer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "TdcCustomer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "TdcCustomer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ward",
                table: "TdcCustomer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "District",
                table: "TdcAuthCustomerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "TdcAuthCustomerDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "TdcAuthCustomerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ward",
                table: "TdcAuthCustomerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
