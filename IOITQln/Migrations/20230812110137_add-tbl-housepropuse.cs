using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblhousepropuse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ Md167HousePropose",
                table: " Md167HousePropose");

            migrationBuilder.RenameTable(
                name: " Md167HousePropose",
                newName: "Md167HousePropose");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Md167HousePropose",
                table: "Md167HousePropose",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Md167HousePropose",
                table: "Md167HousePropose");

            migrationBuilder.RenameTable(
                name: "Md167HousePropose",
                newName: " Md167HousePropose");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ Md167HousePropose",
                table: " Md167HousePropose",
                column: "Id");
        }
    }
}
