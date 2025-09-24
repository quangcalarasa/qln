using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbltdcPriceRents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Customer",
                table: "TdcPriceRent");

            migrationBuilder.AddColumn<int>(
                name: "TdcCustomerId",
                table: "TdcPriceRent",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcCustomerId",
                table: "TdcPriceRent");

            migrationBuilder.AddColumn<string>(
                name: "Customer",
                table: "TdcPriceRent",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
