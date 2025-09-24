using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_PricingApartmentLandDetailaddfieldsnrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "InLimitArea",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "InLimitPercent",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LanscapeAreaLimit",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LanscapeLimitId",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "OutLimitArea",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "OutLimitPercent",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SumLimitArea",
                table: "PricingApartmentLandDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InLimitArea",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "InLimitPercent",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "LanscapeAreaLimit",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "LanscapeLimitId",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "OutLimitArea",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "OutLimitPercent",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "SumLimitArea",
                table: "PricingApartmentLandDetail");
        }
    }
}
