using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtextbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Text1",
                table: "Md167PositionValue",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text2",
                table: "Md167PositionValue",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text3",
                table: "Md167PositionValue",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text4",
                table: "Md167PositionValue",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Text1",
                table: "Md167PositionValue");

            migrationBuilder.DropColumn(
                name: "Text2",
                table: "Md167PositionValue");

            migrationBuilder.DropColumn(
                name: "Text3",
                table: "Md167PositionValue");

            migrationBuilder.DropColumn(
                name: "Text4",
                table: "Md167PositionValue");
        }
    }
}
