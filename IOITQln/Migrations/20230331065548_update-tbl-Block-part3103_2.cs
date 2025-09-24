using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblBlockpart3103_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceListItemId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PriceListValue",
                table: "Block");

            migrationBuilder.AddColumn<int>(
                name: "LandPriceItemId",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LandPriceItemValue",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandPriceItemId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemValue",
                table: "Block");

            migrationBuilder.AddColumn<int>(
                name: "PriceListItemId",
                table: "Block",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PriceListValue",
                table: "Block",
                type: "float",
                nullable: true);
        }
    }
}
