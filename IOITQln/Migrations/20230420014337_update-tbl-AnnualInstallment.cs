using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblAnnualInstallment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "AnnualInstallment");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AnnualInstallment");

            migrationBuilder.AddColumn<DateTime>(
                name: "DoApply",
                table: "AnnualInstallment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UnitPriceId",
                table: "AnnualInstallment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Value",
                table: "AnnualInstallment",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoApply",
                table: "AnnualInstallment");

            migrationBuilder.DropColumn(
                name: "UnitPriceId",
                table: "AnnualInstallment");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "AnnualInstallment");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AnnualInstallment",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AnnualInstallment",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
