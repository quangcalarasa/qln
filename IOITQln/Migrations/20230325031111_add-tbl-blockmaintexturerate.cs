using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblblockmaintexturerate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockMaintextureRate",
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
                    BlockId = table.Column<int>(nullable: false),
                    LevelBlockId = table.Column<int>(nullable: false),
                    TypeMainTexTure = table.Column<int>(nullable: false),
                    CurrentStateMainTextureId = table.Column<int>(nullable: false),
                    RemainingRate = table.Column<float>(nullable: false),
                    MainRate = table.Column<float>(nullable: false),
                    TotalValue = table.Column<float>(nullable: false),
                    TotalValue1 = table.Column<float>(nullable: false),
                    TotalValue2 = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockMaintextureRate", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockMaintextureRate");
        }
    }
}
