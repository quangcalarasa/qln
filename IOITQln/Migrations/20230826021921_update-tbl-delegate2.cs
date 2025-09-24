using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbldelegate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutInfo",
                table: "Md167Delegate",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutInfo",
                table: "Md167Auctioneer",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutInfo",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "AutInfo",
                table: "Md167Auctioneer");
        }
    }
}
