using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblMd167LandTax_ModifyFieldAreaToTypeArea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Md167LandTax");

            migrationBuilder.AddColumn<string>(
                name: "TypeArea",
                table: "Md167LandTax",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeArea",
                table: "Md167LandTax");

            migrationBuilder.AddColumn<decimal>(
                name: "Area",
                table: "Md167LandTax",
                type: "decimal(18,5)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
