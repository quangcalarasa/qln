using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Doc",
                table: "Customer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceCode",
                table: "Customer",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Doc",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PlaceCode",
                table: "Customer");
        }
    }
}
