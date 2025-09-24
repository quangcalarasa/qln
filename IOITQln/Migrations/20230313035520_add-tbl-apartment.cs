using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblapartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apartment",
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
                    TypeHousing = table.Column<int>(nullable: false),
                    BlockId = table.Column<int>(nullable: false),
                    Code = table.Column<string>(maxLength: 1000, nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Address = table.Column<string>(maxLength: 4000, nullable: true),
                    Lane = table.Column<int>(nullable: false),
                    Ward = table.Column<int>(nullable: false),
                    District = table.Column<int>(nullable: false),
                    Province = table.Column<int>(nullable: false),
                    ConstructionAreaValue = table.Column<float>(nullable: false),
                    ConstructionAreaValue1 = table.Column<float>(nullable: true),
                    ConstructionAreaValue2 = table.Column<float>(nullable: true),
                    ConstructionAreaValue3 = table.Column<float>(nullable: true),
                    UseAreaValue = table.Column<float>(nullable: false),
                    UseAreaValue1 = table.Column<float>(nullable: true),
                    UseAreaValue2 = table.Column<float>(nullable: true),
                    LandscapeAreaValue = table.Column<float>(nullable: false),
                    LandscapeAreaValue1 = table.Column<float>(nullable: true),
                    LandscapeAreaValue2 = table.Column<float>(nullable: true),
                    LandscapeAreaValue3 = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApartmentDetail",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    ApartmentId = table.Column<int>(nullable: false),
                    LevelApartment = table.Column<int>(nullable: false),
                    AreaId = table.Column<int>(nullable: false),
                    Floor = table.Column<int>(nullable: false),
                    GeneralAreaValue = table.Column<float>(nullable: true),
                    PeronalAreaValue = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApartmentDetail", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apartment");

            migrationBuilder.DropTable(
                name: "ApartmentDetail");
        }
    }
}
