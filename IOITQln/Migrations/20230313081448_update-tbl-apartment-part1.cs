using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblapartmentpart1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeronalAreaValue",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<float>(
                name: "PersonalAreaValue",
                table: "ApartmentDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonalAreaValue",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<float>(
                name: "PeronalAreaValue",
                table: "ApartmentDetail",
                type: "real",
                nullable: true);
        }
    }
}
