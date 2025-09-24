using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbldistributionfloorcoefficientdetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeFloor",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.AddColumn<float>(
                name: "Value1",
                table: "DistributionFloorCoefficientDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Value2",
                table: "DistributionFloorCoefficientDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Value3",
                table: "DistributionFloorCoefficientDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Value4",
                table: "DistributionFloorCoefficientDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Value5",
                table: "DistributionFloorCoefficientDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Value6",
                table: "DistributionFloorCoefficientDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value1",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropColumn(
                name: "Value2",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropColumn(
                name: "Value3",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropColumn(
                name: "Value4",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropColumn(
                name: "Value5",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropColumn(
                name: "Value6",
                table: "DistributionFloorCoefficientDetail");

            migrationBuilder.AddColumn<int>(
                name: "TypeFloor",
                table: "DistributionFloorCoefficientDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Value",
                table: "DistributionFloorCoefficientDetail",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
