using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblChangeTdcPriceRentDataExcel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcPriceRentExcelMeta",
                table: "TdcPriceRentDataExcel");

            migrationBuilder.AddColumn<int>(
                name: "TdcPriceRentExcelMetaId",
                table: "TdcPriceRentDataExcel",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TdcPriceRentExcelMetaId",
                table: "TdcPriceRentDataExcel");

            migrationBuilder.AddColumn<int>(
                name: "TdcPriceRentExcelMeta",
                table: "TdcPriceRentDataExcel",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
