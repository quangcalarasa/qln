using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatereportapartment2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TdcApartmentArea",
                table: "TdcApartmentManagers",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TdcApartmentCountRoom",
                table: "TdcApartmentManagers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcApartmentArea",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "TdcApartmentCountRoom",
                table: "TdcApartmentManagers");
        }
    }
}
