using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class platformtdcrpup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TdcLength",
                table: "TdcPlatformManagers",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TdcWidth",
                table: "TdcPlatformManagers",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcLength",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "TdcWidth",
                table: "TdcPlatformManagers");
        }
    }
}
