using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Md167Contract_Md167AuctionDecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileCode",
                table: "Md167Contract",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuctionUnit",
                table: "Md167AuctionDecision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileCode",
                table: "Md167Contract");

            migrationBuilder.DropColumn(
                name: "AuctionUnit",
                table: "Md167AuctionDecision");
        }
    }
}
