using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class suaychtemailnhacno : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeSupportReq",
                table: "ExtraSupportRequests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ExtraEmailDebt",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeSupportReq",
                table: "ExtraSupportRequests");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ExtraEmailDebt");
        }
    }
}
