using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblchangeDataRentFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseAreaValue",
                table: "RentFile");

            migrationBuilder.AddColumn<string>(
                name: "CodeCN",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "UseAreaValueCH",
                table: "RentFile",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "UseAreaValueCN",
                table: "RentFile",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "fullAddressCN",
                table: "RentFile",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeCN",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "UseAreaValueCH",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "UseAreaValueCN",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "fullAddressCN",
                table: "RentFile");

            migrationBuilder.AddColumn<float>(
                name: "UseAreaValue",
                table: "RentFile",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
