using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbl_Md167Receipt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Md167PaymentId",
                table: "Md167Debt");

            migrationBuilder.AlterColumn<int>(
                name: "TypeRow",
                table: "Md167Debt",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<long>(
                name: "Md167ReceiptId",
                table: "Md167Debt",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Md167Receipt",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Md167ContractId = table.Column<int>(nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateOfReceipt = table.Column<DateTime>(nullable: true),
                    ReceiptCode = table.Column<string>(maxLength: 500, nullable: true),
                    Note = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Receipt", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Md167Receipt");

            migrationBuilder.DropColumn(
                name: "Md167ReceiptId",
                table: "Md167Debt");

            migrationBuilder.AlterColumn<byte>(
                name: "TypeRow",
                table: "Md167Debt",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<long>(
                name: "Md167PaymentId",
                table: "Md167Debt",
                type: "bigint",
                nullable: true);
        }
    }
}
