using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Block_addfieldssell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "SellConstructionAreaNote",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellConstructionAreaValue",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellConstructionAreaNote",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "SellConstructionAreaValue",
                table: "Block");
        }
    }
}
