using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Apartment_ApartmentDetail_ApartmentLandDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CoefficientDistribution",
                table: "ApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorApplyPriceChange",
                table: "ApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyInvestmentRate",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FloorApplyPriceChange",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedForConstructionOnTheApartmentYard",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedForConstructionOnTheApartmentYardLandscape",
                table: "Apartment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoefficientDistribution",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "FloorApplyPriceChange",
                table: "ApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "ApplyInvestmentRate",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "FloorApplyPriceChange",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "ApprovedForConstructionOnTheApartmentYard",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "ApprovedForConstructionOnTheApartmentYardLandscape",
                table: "Apartment");
        }
    }
}
