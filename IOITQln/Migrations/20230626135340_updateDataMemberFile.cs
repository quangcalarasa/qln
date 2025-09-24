using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updateDataMemberFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCH",
                table: "RentFile");

            migrationBuilder.AddColumn<bool>(
                name: "Check",
                table: "MemberRentFile",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Check",
                table: "MemberRentFile");

            migrationBuilder.AddColumn<string>(
                name: "AddressCH",
                table: "RentFile",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
