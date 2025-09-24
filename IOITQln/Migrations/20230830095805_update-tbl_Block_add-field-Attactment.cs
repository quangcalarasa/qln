using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Block_addfieldAttactment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Attactment",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserIdCreateAttactment",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attactment",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "UserIdCreateAttactment",
                table: "Block");
        }
    }
}
