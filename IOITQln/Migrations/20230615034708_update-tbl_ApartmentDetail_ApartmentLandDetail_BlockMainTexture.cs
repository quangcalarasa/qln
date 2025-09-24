using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_ApartmentDetail_ApartmentLandDetail_BlockMainTexture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "BlockMaintextureRate");

            migrationBuilder.DropColumn(
                name: "ApartmentId",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "ApartmentId",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "BlockMaintextureRate",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "ApartmentLandDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "ApartmentDetail",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "BlockMaintextureRate");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<int>(
                name: "BlockId",
                table: "BlockMaintextureRate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentId",
                table: "ApartmentLandDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentId",
                table: "ApartmentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
