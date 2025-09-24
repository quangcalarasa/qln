using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Pricing_addfieldsFlatCoefficient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "FlatCoefficientId_34",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlatCoefficientId_61",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlatCoefficientId_99",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "FlatCoefficient_34",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "FlatCoefficient_61",
                table: "Pricing",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "FlatCoefficient_99",
                table: "Pricing",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlatCoefficientId_34",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "FlatCoefficientId_61",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "FlatCoefficientId_99",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "FlatCoefficient_34",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "FlatCoefficient_61",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "FlatCoefficient_99",
                table: "Pricing");

            migrationBuilder.AddColumn<double>(
                name: "ApartmentCoefficient_34",
                table: "Pricing",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ApartmentCoefficient_61",
                table: "Pricing",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ApartmentCoefficient_99",
                table: "Pricing",
                type: "float",
                nullable: true);
        }
    }
}
