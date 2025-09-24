using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbl_LandscapeLimit_LandscapeLimitItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanscapeAreaLimit",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "LanscapeLimitId",
                table: "PricingApartmentLandDetail");

            migrationBuilder.AddColumn<float>(
                name: "LandscapeAreaLimit",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLimitItemId",
                table: "PricingApartmentLandDetail",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LandscapeLimit",
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
                    TypeReportApply = table.Column<int>(nullable: false),
                    DecreeType1Id = table.Column<int>(nullable: true),
                    DecreeType2Id = table.Column<int>(nullable: true),
                    Note = table.Column<string>(maxLength: 4000, nullable: true),
                    ProvinceId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandscapeLimit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandscapeLimitItem",
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
                    LanscapeLimitId = table.Column<int>(nullable: false),
                    DistrictId = table.Column<int>(nullable: true),
                    LimitAreaNormal = table.Column<float>(nullable: true),
                    LimitAreaSpecial = table.Column<float>(nullable: true),
                    InLimitPercent = table.Column<float>(nullable: true),
                    OutLimitPercent = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandscapeLimitItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandscapeLimit");

            migrationBuilder.DropTable(
                name: "LandscapeLimitItem");

            migrationBuilder.DropColumn(
                name: "LandscapeAreaLimit",
                table: "PricingApartmentLandDetail");

            migrationBuilder.DropColumn(
                name: "LandscapeLimitItemId",
                table: "PricingApartmentLandDetail");

            migrationBuilder.AddColumn<float>(
                name: "LanscapeAreaLimit",
                table: "PricingApartmentLandDetail",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LanscapeLimitId",
                table: "PricingApartmentLandDetail",
                type: "int",
                nullable: true);
        }
    }
}
