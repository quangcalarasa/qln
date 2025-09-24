using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Md167Contract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Md167Contract",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Md167Contract",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Md167Contract");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Md167Contract");
        }
    }
}
