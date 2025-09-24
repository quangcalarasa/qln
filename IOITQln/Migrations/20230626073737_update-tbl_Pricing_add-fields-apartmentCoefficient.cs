using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Pricing_addfieldsapartmentCoefficient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AreaCorrectionCoefficientValue",
                table: "Pricing",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<double>(
                name: "ApartmentCoefficient_34",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ApartmentCoefficient_61",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ApartmentCoefficient_99",
                table: "Pricing",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentCoefficient_34",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "ApartmentCoefficient_61",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "ApartmentCoefficient_99",
                table: "Pricing");

            migrationBuilder.AlterColumn<double>(
                name: "AreaCorrectionCoefficientValue",
                table: "Pricing",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);
        }
    }
}
