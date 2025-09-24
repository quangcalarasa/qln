using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbllandcoefficient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AreaCorrectionCoefficient",
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
                    ParentId = table.Column<int>(nullable: false),
                    DecreeType1Id = table.Column<int>(nullable: false),
                    DecreeType2Id = table.Column<int>(nullable: false),
                    Des = table.Column<string>(maxLength: 4000, nullable: true),
                    NameOfConstruction = table.Column<string>(maxLength: 2000, nullable: true),
                    Note = table.Column<string>(maxLength: 4000, nullable: true),
                    Value = table.Column<double>(nullable: false),
                    DoApply = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaCorrectionCoefficient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandPrice",
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
                    DecreeType1Id = table.Column<int>(nullable: false),
                    DecreeType2Id = table.Column<int>(nullable: true),
                    LaneId = table.Column<int>(nullable: false),
                    LaneStartId = table.Column<int>(nullable: false),
                    LaneEndId = table.Column<int>(nullable: false),
                    UnitPriceId = table.Column<int>(nullable: false),
                    Value = table.Column<double>(nullable: false),
                    Des = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandPrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandSpecialCoefficient",
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
                    DecreeType1Id = table.Column<int>(nullable: false),
                    DecreeType2Id = table.Column<int>(nullable: true),
                    DoApply = table.Column<DateTime>(nullable: true),
                    Value1 = table.Column<double>(nullable: false),
                    Value2 = table.Column<double>(nullable: false),
                    Value3 = table.Column<double>(nullable: false),
                    Value4 = table.Column<double>(nullable: false),
                    Value5 = table.Column<double>(nullable: false),
                    Value6 = table.Column<double>(nullable: false),
                    Value7 = table.Column<double>(nullable: false),
                    Value8 = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandSpecialCoefficient", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaCorrectionCoefficient");

            migrationBuilder.DropTable(
                name: "LandPrice");

            migrationBuilder.DropTable(
                name: "LandSpecialCoefficient");
        }
    }
}
