using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class deleteidentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "TdcApartmentManagers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "TdcPlatformManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "TdcApartmentManagers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
