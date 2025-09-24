using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_ApartmentLandDetail_add_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FloorApplyCoefficient",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "ApartmentLandDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FloorId",
                table: "ApartmentLandDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ApartmentLandDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "FloorId",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ApartmentLandDetail");

            migrationBuilder.AddColumn<int>(
                name: "FloorApplyCoefficient",
                table: "ApartmentDetail",
                type: "int",
                nullable: true);
        }
    }
}
