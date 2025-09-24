using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class delete_districtid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "TdcApartmentManagers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "TdcPlatformManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "TdcApartmentManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
