using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblNocReceipt_updateDebtsTable_removePromissory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Promissory");

            migrationBuilder.DropColumn(
                name: "PromissoryId",
                table: "DebtsTable");

            migrationBuilder.AddColumn<Guid>(
                name: "NocReceiptId",
                table: "DebtsTable",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NocReceipt",
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
                    Number = table.Column<string>(maxLength: 500, nullable: true),
                    Code = table.Column<string>(maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Executor = table.Column<string>(maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Action = table.Column<byte>(nullable: false),
                    NumberOfTransfer = table.Column<string>(maxLength: 1000, nullable: true),
                    InvoiceCode = table.Column<string>(maxLength: 1000, nullable: true),
                    DateOfTransfer = table.Column<DateTime>(nullable: true),
                    Note = table.Column<string>(maxLength: 2000, nullable: true),
                    Content = table.Column<string>(maxLength: 2000, nullable: true),
                    IsImportExcel = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NocReceipt", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NocReceipt");

            migrationBuilder.DropColumn(
                name: "NocReceiptId",
                table: "DebtsTable");

            migrationBuilder.AddColumn<int>(
                name: "PromissoryId",
                table: "DebtsTable",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Promissory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<byte>(type: "tinyint", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfTransfer = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Executor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InvoiceCode = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsImportExcel = table.Column<bool>(type: "bit", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Number = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumberOfTransfer = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promissory", x => x.Id);
                });
        }
    }
}
