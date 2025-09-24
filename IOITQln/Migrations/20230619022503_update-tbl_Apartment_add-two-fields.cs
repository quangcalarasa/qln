using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Apartment_addtwofields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConstructionAreaNote",
                table: "Apartment",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UseAreaNote",
                table: "Apartment",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConstructionAreaNote",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "UseAreaNote",
                table: "Apartment");
        }
    }
}
