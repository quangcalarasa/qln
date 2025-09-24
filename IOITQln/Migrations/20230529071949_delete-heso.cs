using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class deleteheso : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TDCProjectIngrePriceId",
                table: "TdcPriceOneSellTemporary");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TDCProjectIngrePriceId",
                table: "TdcPriceOneSellTemporary",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
