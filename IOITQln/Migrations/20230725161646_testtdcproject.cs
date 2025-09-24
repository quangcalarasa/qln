using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class testtdcproject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TDCprojectests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    LandCount = table.Column<int>(nullable: false),
                    FullAddress = table.Column<string>(nullable: true),
                    HouseNumber = table.Column<string>(nullable: true),
                    Lane = table.Column<string>(nullable: true),
                    Ward = table.Column<string>(nullable: true),
                    District = table.Column<string>(nullable: true),
                    Province = table.Column<string>(nullable: true),
                    BuildingName = table.Column<string>(nullable: true),
                    TotalAreas = table.Column<double>(nullable: false),
                    TotalApartment = table.Column<int>(nullable: false),
                    TotalPlatform = table.Column<int>(nullable: false),
                    TotalFloorAreas = table.Column<double>(nullable: false),
                    TotalUseAreas = table.Column<double>(nullable: false),
                    TotalBuildAreas = table.Column<double>(nullable: false),
                    DebtRate = table.Column<decimal>(nullable: false),
                    LateRate = table.Column<decimal>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCprojectests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TDCprojectests");
        }
    }
}
