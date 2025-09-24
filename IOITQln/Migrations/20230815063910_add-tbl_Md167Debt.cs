using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbl_Md167Debt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Md167Debt",
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
                    HouseCode = table.Column<string>(maxLength: 500, nullable: true),
                    Md167ContractId = table.Column<int>(nullable: false),
                    TypeRow = table.Column<byte>(nullable: false),
                    Index = table.Column<int>(nullable: true),
                    Title = table.Column<string>(maxLength: 2000, nullable: true),
                    DopExpected = table.Column<DateTime>(nullable: true),
                    DopActual = table.Column<DateTime>(nullable: true),
                    InterestCalcDate = table.Column<int>(nullable: true),
                    Interest = table.Column<float>(nullable: true),
                    AmountPaidPerMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountPaidInPeriod = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountToBePaid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountDiff = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(maxLength: 2000, nullable: true),
                    Md167PaymentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Debt", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Md167Debt");
        }
    }
}
