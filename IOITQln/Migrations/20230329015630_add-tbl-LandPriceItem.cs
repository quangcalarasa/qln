using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblLandPriceItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaneEndId",
                table: "LandPrice");

            migrationBuilder.DropColumn(
                name: "LaneId",
                table: "LandPrice");

            migrationBuilder.DropColumn(
                name: "LaneStartId",
                table: "LandPrice");

            migrationBuilder.DropColumn(
                name: "UnitPriceId",
                table: "LandPrice");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "LandPrice");

            migrationBuilder.AddColumn<int>(
                name: "District",
                table: "LandPrice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "LandPrice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LandPriceItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    LandPriceId = table.Column<int>(nullable: false),
                    LaneId = table.Column<int>(nullable: false),
                    LaneStartId = table.Column<int>(nullable: false),
                    LaneEndId = table.Column<int>(nullable: false),
                    Value = table.Column<double>(nullable: false),
                    Des = table.Column<string>(maxLength: 4000, nullable: true),
                    Ward = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandPriceItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandPriceItem");

            migrationBuilder.DropColumn(
                name: "District",
                table: "LandPrice");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "LandPrice");

            migrationBuilder.AddColumn<int>(
                name: "LaneEndId",
                table: "LandPrice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneId",
                table: "LandPrice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneStartId",
                table: "LandPrice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitPriceId",
                table: "LandPrice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Value",
                table: "LandPrice",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
