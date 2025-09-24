using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Block_TypeReportApply4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentTypeReportApply",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "ParentTypeReportApply",
                table: "Block");
        }
    }
}
