using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblLandPriceItempart1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaneId",
                table: "LandPriceItem");

            migrationBuilder.AddColumn<string>(
                name: "LaneName",
                table: "LandPriceItem",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaneName",
                table: "LandPriceItem");

            migrationBuilder.AddColumn<int>(
                name: "LaneId",
                table: "LandPriceItem",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
