using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_PricingApartmentLandDetailaddfieldSumLimitAreaStr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SumLimitAreaStr",
                table: "PricingApartmentLandDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SumLimitAreaStr",
                table: "PricingApartmentLandDetail");
        }
    }
}
