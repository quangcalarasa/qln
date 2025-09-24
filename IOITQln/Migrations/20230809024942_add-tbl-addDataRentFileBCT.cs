using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbladdDataRentFileBCT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Area",
                table: "RentFileBCT",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEnd",
                table: "RentFileBCT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStart",
                table: "RentFileBCT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "DebtsTable",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "RentFileBCT");

            migrationBuilder.DropColumn(
                name: "DateEnd",
                table: "RentFileBCT");

            migrationBuilder.DropColumn(
                name: "DateStart",
                table: "RentFileBCT");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "DebtsTable");
        }
    }
}
