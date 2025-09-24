using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblsTDCProddject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TDCProjectId",
                table: "TDCProjectPriceAndTax",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TDCProjectId",
                table: "TDCProjectIngrePrice",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TDCProjectId",
                table: "TDCProjectPriceAndTax");

            migrationBuilder.DropColumn(
                name: "TDCProjectId",
                table: "TDCProjectIngrePrice");
        }
    }
}
