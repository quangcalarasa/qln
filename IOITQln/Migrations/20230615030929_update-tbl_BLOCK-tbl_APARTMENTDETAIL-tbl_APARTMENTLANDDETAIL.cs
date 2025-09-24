using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_BLOCKtbl_APARTMENTDETAILtbl_APARTMENTLANDDETAIL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "LandscapeAreaValue",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandscapePrivateAreaValue",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ApartmentLandDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ApartmentDetail",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandscapeAreaValue",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapePrivateAreaValue",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ApartmentDetail");
        }
    }
}
