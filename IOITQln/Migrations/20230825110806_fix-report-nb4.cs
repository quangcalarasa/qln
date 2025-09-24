using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixreportnb4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HandOverOther",
                table: "TdcPlatformManagers",
                newName: "HandoverOther");

            migrationBuilder.RenameColumn(
                name: "HandOverCenter",
                table: "TdcPlatformManagers",
                newName: "HandoverCenter");

            migrationBuilder.RenameColumn(
                name: "HandOverCenter",
                table: "TdcApartmentManagers",
                newName: "HandoverCenter");

            migrationBuilder.AddColumn<int>(
                name: "RealityNumber",
                table: "TdcPlatformManagers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RealityNumber",
                table: "TdcApartmentManagers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealityNumber",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "RealityNumber",
                table: "TdcApartmentManagers");

            migrationBuilder.RenameColumn(
                name: "HandoverOther",
                table: "TdcPlatformManagers",
                newName: "HandOverOther");

            migrationBuilder.RenameColumn(
                name: "HandoverCenter",
                table: "TdcPlatformManagers",
                newName: "HandOverCenter");

            migrationBuilder.RenameColumn(
                name: "HandoverCenter",
                table: "TdcApartmentManagers",
                newName: "HandOverCenter");
        }
    }
}
