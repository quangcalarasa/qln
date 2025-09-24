using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblblock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Block",
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
                    FormHousing = table.Column<int>(nullable: false),
                    FloorApplyPriceChange = table.Column<int>(nullable: false),
                    LandNo = table.Column<string>(maxLength: 4000, nullable: false),
                    MapNo = table.Column<string>(maxLength: 4000, nullable: false),
                    LandscapeLocation = table.Column<int>(nullable: false),
                    LevelAlley = table.Column<int>(nullable: false),
                    LandscapeLocationInAlley = table.Column<int>(nullable: false),
                    IsAlley = table.Column<bool>(nullable: true),
                    Width = table.Column<float>(nullable: false),
                    Deep = table.Column<float>(nullable: false),
                    Code = table.Column<string>(maxLength: 1000, nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Address = table.Column<string>(maxLength: 4000, nullable: true),
                    Lane = table.Column<int>(nullable: false),
                    Ward = table.Column<int>(nullable: false),
                    District = table.Column<int>(nullable: false),
                    Province = table.Column<int>(nullable: false),
                    TypePile = table.Column<byte>(nullable: false),
                    ConstructionAreaNote = table.Column<string>(maxLength: 4000, nullable: false),
                    ConstructionAreaValue1 = table.Column<float>(nullable: true),
                    ConstructionAreaValue2 = table.Column<float>(nullable: true),
                    ConstructionAreaValue3 = table.Column<float>(nullable: true),
                    UseAreaNote = table.Column<string>(maxLength: 4000, nullable: false),
                    UseAreaValue1 = table.Column<float>(nullable: true),
                    UseAreaValue2 = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorBlockMap",
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
                    BlockId = table.Column<int>(nullable: false),
                    FloorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorBlockMap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LevelBlockMap",
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
                    BlockId = table.Column<int>(nullable: false),
                    LevelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelBlockMap", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Block");

            migrationBuilder.DropTable(
                name: "FloorBlockMap");

            migrationBuilder.DropTable(
                name: "LevelBlockMap");
        }
    }
}
