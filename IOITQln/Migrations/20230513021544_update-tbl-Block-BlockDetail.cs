using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblBlockBlockDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "YardArea",
                table: "BlockDetail",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Blueprint",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CampusArea",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Dispute",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstablishStateOwnership",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeBlockEntity",
                table: "Block",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsageStatus",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageStatusNote",
                table: "Block",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UseAreaNote1",
                table: "Block",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YardArea",
                table: "BlockDetail");

            migrationBuilder.DropColumn(
                name: "Blueprint",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "CampusArea",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "Dispute",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "EstablishStateOwnership",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TypeBlockEntity",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "UsageStatus",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "UsageStatusNote",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "UseAreaNote1",
                table: "Block");
        }
    }
}
