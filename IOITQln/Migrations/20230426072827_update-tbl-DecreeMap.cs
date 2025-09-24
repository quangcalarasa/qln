using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblDecreeMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeId",
                table: "DecreeMap");

            migrationBuilder.AddColumn<int>(
                name: "DecreeType1Id",
                table: "DecreeMap",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeType1Id",
                table: "DecreeMap");

            migrationBuilder.AddColumn<int>(
                name: "DecreeId",
                table: "DecreeMap",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
