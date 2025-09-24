using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblRentBCTTableAndChilDftable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WardId",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChilDfTable",
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
                    RentBctTableId = table.Column<Guid>(nullable: false),
                    CoefficientId = table.Column<int>(nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChilDfTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RentBctTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    AreaName = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: false),
                    PrivateArea = table.Column<float>(nullable: false),
                    DateCoefficient = table.Column<DateTime>(nullable: false),
                    StandardPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalK = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ktlcb = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ktdbt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceRent1m2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(nullable: true),
                    DateStart = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false),
                    DateDiff = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    VAT = table.Column<double>(nullable: false),
                    PolicyReduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    check = table.Column<bool>(nullable: false),
                    RentFileId = table.Column<int>(nullable: false),
                    PriceVAT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthDiff = table.Column<int>(nullable: false),
                    TotalPrice = table.Column<decimal>(nullable: false),
                    PriceAfterDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountCoff = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentBctTable", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChilDfTable");

            migrationBuilder.DropTable(
                name: "RentBctTable");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "RentFile");
        }
    }
}
