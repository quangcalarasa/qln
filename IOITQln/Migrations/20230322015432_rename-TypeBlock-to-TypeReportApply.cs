using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class renameTypeBlocktoTypeReportApply : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeHousing",
                table: "UseValueCoefficient");

            migrationBuilder.DropColumn(
                name: "TypeHousing",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TypeHousing",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "TypeHousing",
                table: "Apartment");

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "UseValueCoefficient",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "Block",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "Area",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "Apartment",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "UseValueCoefficient");

            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "Apartment");

            migrationBuilder.AddColumn<int>(
                name: "TypeHousing",
                table: "UseValueCoefficient",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeHousing",
                table: "Block",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeHousing",
                table: "Area",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeHousing",
                table: "Apartment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
