using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblwarddistrict : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OldName",
                table: "Ward",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldName",
                table: "District",
                type: "ntext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldName",
                table: "Ward");

            migrationBuilder.DropColumn(
                name: "OldName",
                table: "District");
        }
    }
}
