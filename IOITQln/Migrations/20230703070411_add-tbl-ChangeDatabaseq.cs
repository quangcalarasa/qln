using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblChangeDatabaseq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeId",
                table: "ConversionCoefficient");

            migrationBuilder.AddColumn<int>(
                name: "TypeQD",
                table: "ConversionCoefficient",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeQD",
                table: "ConversionCoefficient");

            migrationBuilder.AddColumn<int>(
                name: "DecreeId",
                table: "ConversionCoefficient",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
