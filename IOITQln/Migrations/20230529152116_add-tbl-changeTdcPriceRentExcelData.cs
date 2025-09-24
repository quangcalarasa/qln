using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblchangeTdcPriceRentExcelData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayTimeId",
                table: "TdcPriceRentDataExcel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayTimeId",
                table: "TdcPriceRentDataExcel",
                type: "int",
                nullable: true);
        }
    }
}
