using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblPromissory3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentFilesId",
                table: "Promissory");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Promissory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Promissory");

            migrationBuilder.AddColumn<int>(
                name: "RentFilesId",
                table: "Promissory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
