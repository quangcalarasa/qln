using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblblockfield_61 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAlley_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontOfLine_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocationInAlley_61",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlley_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "IsFrontOfLine_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocationInAlley_61",
                table: "Block");
        }
    }
}
