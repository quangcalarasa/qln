using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblAddNewEntitiesPriceRent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAreaCT",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAreaTT",
                table: "TdcPriceRent",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAreaCT",
                table: "TdcPriceRent");

            migrationBuilder.DropColumn(
                name: "TotalAreaTT",
                table: "TdcPriceRent");
        }
    }
}
