using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Md167Valuation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Md167Valuation");

            migrationBuilder.AddColumn<string>(
                name: "UnitValuation",
                table: "Md167Valuation",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitValuation",
                table: "Md167Valuation");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Md167Valuation",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
