using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblarea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaValue",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "GeneralAreaValue",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "PeronalAreaValue",
                table: "Area");

            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "Area");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "AreaValue",
                table: "Area",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "BlockId",
                table: "Area",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "GeneralAreaValue",
                table: "Area",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PeronalAreaValue",
                table: "Area",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "Area",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
