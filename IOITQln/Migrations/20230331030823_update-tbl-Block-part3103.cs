using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblBlockpart3103 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "Block");

            migrationBuilder.AddColumn<int>(
                name: "PriceListItemId",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceListItemId",
                table: "Block");

            migrationBuilder.AddColumn<int>(
                name: "PriceListId",
                table: "Block",
                type: "int",
                nullable: true);
        }
    }
}
