using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblApartmentDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coefficient",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<float>(
                name: "CoefficientDistribution",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CoefficientUseValue",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecreeType1Id",
                table: "ApartmentDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TermApply",
                table: "ApartmentDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoefficientDistribution",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "CoefficientUseValue",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "DecreeType1Id",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "TermApply",
                table: "ApartmentDetail");

            migrationBuilder.AddColumn<float>(
                name: "Coefficient",
                table: "ApartmentDetail",
                type: "real",
                nullable: true);
        }
    }
}
