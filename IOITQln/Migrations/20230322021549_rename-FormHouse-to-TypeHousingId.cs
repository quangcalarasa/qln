using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class renameFormHousetoTypeHousingId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormHousing",
                table: "Block");

            migrationBuilder.AddColumn<int>(
                name: "TypeHousingId",
                table: "Block",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeHousingId",
                table: "Block");

            migrationBuilder.AddColumn<int>(
                name: "FormHousing",
                table: "Block",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
