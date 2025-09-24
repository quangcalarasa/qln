using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblImportHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportHistory",
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
                    Type = table.Column<int>(nullable: false),
                    FileUrl = table.Column<string>(maxLength: 2000, nullable: false),
                    DataStorage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportHistory", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportHistory");
        }
    }
}
