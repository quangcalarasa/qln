using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblpricelist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypePile1",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "TypePile2",
                table: "PriceList");

            migrationBuilder.AddColumn<double>(
                name: "ValueTypePile1",
                table: "PriceList",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ValueTypePile2",
                table: "PriceList",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValueTypePile1",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "ValueTypePile2",
                table: "PriceList");

            migrationBuilder.AddColumn<double>(
                name: "TypePile1",
                table: "PriceList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TypePile2",
                table: "PriceList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
