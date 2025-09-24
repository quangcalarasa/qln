using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblBlockaddfiledNo2LandScapePrice_61 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "No2LandScapePrice_61",
                table: "Block",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeAlley_61",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "No2LandScapePrice_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TypeAlley_61",
                table: "Block");
        }
    }
}
