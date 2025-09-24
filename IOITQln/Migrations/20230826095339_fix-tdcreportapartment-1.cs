using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixtdcreportapartment1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistricProjectId",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<int>(
                name: "DistrictProjectId",
                table: "TdcApartmentManagers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HandOver",
                table: "TdcApartmentManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictProjectId",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "HandOver",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<int>(
                name: "DistricProjectId",
                table: "TdcApartmentManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
