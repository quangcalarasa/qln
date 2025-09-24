using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblchildDfs1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ChildDefaultCoefficient");

            migrationBuilder.AddColumn<int>(
                name: "defaultCoefficientsId",
                table: "ChildDefaultCoefficient",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "defaultCoefficientsId",
                table: "ChildDefaultCoefficient");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ChildDefaultCoefficient",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
