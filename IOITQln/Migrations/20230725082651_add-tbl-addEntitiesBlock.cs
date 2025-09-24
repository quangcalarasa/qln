using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbladdEntitiesBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateApply",
                table: "Block",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRecord",
                table: "Block",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Block",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TakeOver",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeHouse",
                table: "Block",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateApply",
                table: "Apartment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRecord",
                table: "Apartment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Apartment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TakeOver",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeHouse",
                table: "Apartment",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateApply",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "DateRecord",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TakeOver",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TypeHouse",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "DateApply",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "DateRecord",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "TakeOver",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "TypeHouse",
                table: "Apartment");
        }
    }
}
