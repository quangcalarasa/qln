using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addupdatetbl_Task_General_DataImport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataExtraStorage",
                table: "ImportHistory",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeEstablishStateOwnership",
                table: "Block",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBlueprint",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEstablishStateOwnership",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBlueprint",
                table: "Block",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "No",
                table: "Block",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeEstablishStateOwnership",
                table: "Apartment",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBlueprint",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEstablishStateOwnership",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBlueprint",
                table: "Apartment",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "No",
                table: "Apartment",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportHistoryItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    ImportHistoryId = table.Column<long>(nullable: false),
                    DataStorage = table.Column<string>(type: "ntext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportHistoryItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportHistoryItem");

            migrationBuilder.DropColumn(
                name: "DataExtraStorage",
                table: "ImportHistory");

            migrationBuilder.DropColumn(
                name: "CodeEstablishStateOwnership",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "DateBlueprint",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "DateEstablishStateOwnership",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "NameBlueprint",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "No",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "CodeEstablishStateOwnership",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "DateBlueprint",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "DateEstablishStateOwnership",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "NameBlueprint",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "No",
                table: "Apartment");
        }
    }
}
