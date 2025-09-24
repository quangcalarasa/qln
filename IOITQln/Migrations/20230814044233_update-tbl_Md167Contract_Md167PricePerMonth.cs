using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbl_Md167Contract_Md167PricePerMonth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateEffect",
                table: "Md167PricePerMonth",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<float>(
                name: "VatValue",
                table: "Md167PricePerMonth",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "AllocationCoefficient",
                table: "Md167Contract",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateEffect",
                table: "Md167PricePerMonth");

            migrationBuilder.DropColumn(
                name: "VatValue",
                table: "Md167PricePerMonth");

            migrationBuilder.DropColumn(
                name: "AllocationCoefficient",
                table: "Md167Contract");
        }
    }
}
