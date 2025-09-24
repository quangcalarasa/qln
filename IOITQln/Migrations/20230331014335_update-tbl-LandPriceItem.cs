using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblLandPriceItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaneEndId",
                table: "LandPriceItem");

            migrationBuilder.DropColumn(
                name: "LaneStartId",
                table: "LandPriceItem");

            migrationBuilder.AddColumn<string>(
                name: "LaneEndName",
                table: "LandPriceItem",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LaneStartName",
                table: "LandPriceItem",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaneEndName",
                table: "LandPriceItem");

            migrationBuilder.DropColumn(
                name: "LaneStartName",
                table: "LandPriceItem");

            migrationBuilder.AddColumn<int>(
                name: "LaneEndId",
                table: "LandPriceItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneStartId",
                table: "LandPriceItem",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
