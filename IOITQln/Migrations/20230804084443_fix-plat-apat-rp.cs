using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixplatapatrp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonNotReceived",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonNotReceived",
                table: "TdcApartmentManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonNotReceived",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "ReasonNotReceived",
                table: "TdcApartmentManagers");
        }
    }
}
