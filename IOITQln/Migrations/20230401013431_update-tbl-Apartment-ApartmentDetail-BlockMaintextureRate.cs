using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblApartmentApartmentDetailBlockMaintextureRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Floor",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "Lane",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Apartment");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BlockMaintextureRate",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Coefficient",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorApplyCoefficient",
                table: "ApartmentDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FloorId",
                table: "ApartmentDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "InLimit40Percent",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OutLimit100Percent",
                table: "Apartment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "BlockMaintextureRate");

            migrationBuilder.DropColumn(
                name: "Coefficient",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "FloorApplyCoefficient",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "FloorId",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "InLimit40Percent",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "OutLimit100Percent",
                table: "Apartment");

            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "ApartmentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "District",
                table: "Apartment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lane",
                table: "Apartment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Apartment",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "Apartment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ward",
                table: "Apartment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
