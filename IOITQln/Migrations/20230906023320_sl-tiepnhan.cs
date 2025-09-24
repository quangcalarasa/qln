using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class sltiepnhan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealityNumber",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<int>(
                name: "ReceiveNumber",
                table: "TdcPlatformManagers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReceiveNumber",
                table: "TdcApartmentManagers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiveNumber",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "ReceiveNumber",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<int>(
                name: "RealityNumber",
                table: "TdcApartmentManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
