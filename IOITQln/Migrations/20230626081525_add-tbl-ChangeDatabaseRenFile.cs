using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblChangeDatabaseRenFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "RentFile");

            migrationBuilder.AddColumn<string>(
                name: "AddressCH",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressKH",
                table: "RentFile",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCH",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "AddressKH",
                table: "RentFile");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "RentFile",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
