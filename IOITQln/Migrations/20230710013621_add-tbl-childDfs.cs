using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblchildDfs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "defaultCoefficientsId",
                table: "ChildDefaultCoefficient");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ChildDefaultCoefficient",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ChildDefaultCoefficient");

            migrationBuilder.AddColumn<int>(
                name: "defaultCoefficientsId",
                table: "ChildDefaultCoefficient",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
