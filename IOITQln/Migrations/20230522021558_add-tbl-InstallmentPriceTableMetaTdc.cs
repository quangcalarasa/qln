using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblInstallmentPriceTableMetaTdc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallmentPriceTableMetaTdcId",
                table: "InstallmentPriceTableTdc",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InstallmentPriceTableMetaTdc",
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
                    TdcIntallmentPriceId = table.Column<int>(nullable: false),
                    DataStatus = table.Column<int>(nullable: false),
                    PayTimeId = table.Column<int>(nullable: true),
                    Pay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Paid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentPriceTableMetaTdc", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstallmentPriceTableMetaTdc");

            migrationBuilder.DropColumn(
                name: "InstallmentPriceTableMetaTdcId",
                table: "InstallmentPriceTableTdc");
        }
    }
}
