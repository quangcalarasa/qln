using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblsTDCProddject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TDCProject",
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
                    Code = table.Column<string>(maxLength: 1000, nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    LandCount = table.Column<string>(nullable: false),
                    HouseNumber = table.Column<string>(nullable: false),
                    Lane = table.Column<int>(nullable: true),
                    Ward = table.Column<int>(nullable: false),
                    District = table.Column<int>(nullable: false),
                    Province = table.Column<int>(nullable: false),
                    BuildingName = table.Column<string>(nullable: true),
                    TotalAreas = table.Column<double>(nullable: false),
                    TotalApartment = table.Column<int>(nullable: false),
                    TotalPlatform = table.Column<int>(nullable: false),
                    TotalFloorAreas = table.Column<double>(nullable: false),
                    TotalUseAreas = table.Column<double>(nullable: false),
                    TotalBuildAreas = table.Column<double>(nullable: false),
                    Note = table.Column<string>(maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCProject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TDCProjectIngrePrice",
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
                    IngredientsPriceId = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false),
                    Location = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCProjectIngrePrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TDCProjectPriceAndTax",
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
                    PriceAndTaxId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCProjectPriceAndTax", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TDCProjectPriceAndTaxDetails",
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
                    PriceAndTaxId = table.Column<int>(nullable: false),
                    IngredientsPriceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TDCProjectPriceAndTaxDetails", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TDCProject");

            migrationBuilder.DropTable(
                name: "TDCProjectIngrePrice");

            migrationBuilder.DropTable(
                name: "TDCProjectPriceAndTax");

            migrationBuilder.DropTable(
                name: "TDCProjectPriceAndTaxDetails");
        }
    }
}
