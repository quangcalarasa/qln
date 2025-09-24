using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblInvestmentRateItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "InvestmentRate");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "InvestmentRate");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "InvestmentRate");

            migrationBuilder.DropColumn(
                name: "Value1",
                table: "InvestmentRate");

            migrationBuilder.DropColumn(
                name: "Value2",
                table: "InvestmentRate");

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "InvestmentRate",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InvestmentRateItem",
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
                    InvestmentRateId = table.Column<int>(nullable: false),
                    LineInfo = table.Column<string>(maxLength: 1000, nullable: true),
                    DetailInfo = table.Column<string>(maxLength: 2000, nullable: true),
                    Value = table.Column<double>(nullable: false),
                    Value1 = table.Column<double>(nullable: false),
                    Value2 = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentRateItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentRateItem");

            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "InvestmentRate");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InvestmentRate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "InvestmentRate",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Value",
                table: "InvestmentRate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Value1",
                table: "InvestmentRate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Value2",
                table: "InvestmentRate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
