using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class qlyxulyycht2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeSupportReqName",
                table: "ExtraSupportRequests");

            migrationBuilder.DropColumn(
                name: "TypeSupportReqName",
                table: "ExtraSupportHandles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeSupportReqName",
                table: "ExtraSupportRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeSupportReqName",
                table: "ExtraSupportHandles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
