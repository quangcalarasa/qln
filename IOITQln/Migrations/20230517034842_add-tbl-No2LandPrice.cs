using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblNo2LandPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "No2LandPrice",
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
                    StartValue = table.Column<double>(nullable: true),
                    EndValue = table.Column<double>(nullable: true),
                    MainPriceLess2M = table.Column<double>(nullable: true),
                    ExtraPriceLess2M = table.Column<double>(nullable: true),
                    MainPriceLess3M = table.Column<double>(nullable: true),
                    ExtraPriceLess3M = table.Column<double>(nullable: true),
                    MainPriceLess5M = table.Column<double>(nullable: true),
                    ExtraPriceLess5M = table.Column<double>(nullable: true),
                    MainPriceGreater5M = table.Column<double>(nullable: true),
                    ExtraPriceGreater5M = table.Column<double>(nullable: true),
                    Note = table.Column<string>(maxLength: 4000, nullable: true),
                    TypeUrban = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_No2LandPrice", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "No2LandPrice");
        }
    }
}
