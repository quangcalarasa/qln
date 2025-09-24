using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixphieuchi3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HousePayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    HouseId = table.Column<int>(nullable: false),
                    Md167PaymentId = table.Column<int>(nullable: false),
                    TaxNN = table.Column<decimal>(nullable: true),
                    Paid = table.Column<decimal>(nullable: false),
                    Debt = table.Column<decimal>(nullable: false),
                    HouseName = table.Column<string>(nullable: true),
                    ProviceName = table.Column<string>(nullable: true),
                    DistrictName = table.Column<string>(nullable: true),
                    WardName = table.Column<string>(nullable: true),
                    LaneName = table.Column<string>(nullable: true),
                    TypeHouse = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousePayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ManagePayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Md167HouseId = table.Column<int>(nullable: false),
                    Payment = table.Column<decimal>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ManagePayments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HousePayments");

            migrationBuilder.DropTable(
                name: "Md167ManagePayments");
        }
    }
}
