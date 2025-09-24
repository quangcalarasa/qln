using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblPriceList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMezzanine",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "NameOfConstruction",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "UnitPriceId",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "ValueTypePile1",
                table: "PriceList");

            migrationBuilder.DropColumn(
                name: "ValueTypePile2",
                table: "PriceList");

            migrationBuilder.CreateTable(
                name: "PriceListItem",
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
                    PriceListId = table.Column<int>(nullable: false),
                    NameOfConstruction = table.Column<string>(maxLength: 2000, nullable: false),
                    ValueTypePile1 = table.Column<double>(nullable: false),
                    ValueTypePile2 = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceListItem");

            migrationBuilder.AddColumn<bool>(
                name: "IsMezzanine",
                table: "PriceList",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameOfConstruction",
                table: "PriceList",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "PriceList",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "PriceList",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitPriceId",
                table: "PriceList",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ValueTypePile1",
                table: "PriceList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ValueTypePile2",
                table: "PriceList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
