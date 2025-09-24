using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class themmadinhdanhcannen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "TdcApartmentManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "TdcApartmentManagers");
        }
    }
}
