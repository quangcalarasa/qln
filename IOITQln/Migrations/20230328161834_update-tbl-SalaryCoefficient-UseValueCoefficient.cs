using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblSalaryCoefficientUseValueCoefficient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "UseValueCoefficient");

            migrationBuilder.AddColumn<string>(
                name: "Des",
                table: "UseValueCoefficient",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecreeType1Id",
                table: "SalaryCoefficient",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecreeType2Id",
                table: "SalaryCoefficient",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Des",
                table: "UseValueCoefficient");

            migrationBuilder.DropColumn(
                name: "DecreeType1Id",
                table: "SalaryCoefficient");

            migrationBuilder.DropColumn(
                name: "DecreeType2Id",
                table: "SalaryCoefficient");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "UseValueCoefficient",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}
