using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Promissory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfTransfer",
                table: "Promissory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceCode",
                table: "Promissory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberOfTransfer",
                table: "Promissory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfTransfer",
                table: "Promissory");

            migrationBuilder.DropColumn(
                name: "InvoiceCode",
                table: "Promissory");

            migrationBuilder.DropColumn(
                name: "NumberOfTransfer",
                table: "Promissory");
        }
    }
}
