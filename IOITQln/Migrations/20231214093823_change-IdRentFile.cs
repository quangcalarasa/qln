using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class changeIdRentFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentFile");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressKH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Art = table.Column<bool>(type: "bit", nullable: false),
                    CampusArea = table.Column<float>(type: "real", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeCH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeCN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeHS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeKH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConstructionAreaValue = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DateHD = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileStatus = table.Column<int>(type: "int", nullable: false),
                    IndexC = table.Column<int>(type: "int", nullable: false),
                    IndexH = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessProfileCeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RentApartmentId = table.Column<int>(type: "int", nullable: false),
                    RentBlockId = table.Column<int>(type: "int", nullable: false),
                    SHNN = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    TypeBlockId = table.Column<int>(type: "int", nullable: false),
                    TypeHouse = table.Column<int>(type: "int", nullable: false),
                    TypeReportApply = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    UsageStatus = table.Column<int>(type: "int", nullable: true),
                    UseAreaValueCH = table.Column<float>(type: "real", nullable: false),
                    UseAreaValueCN = table.Column<float>(type: "real", nullable: false),
                    WardId = table.Column<int>(type: "int", nullable: false),
                    fullAddressCH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fullAddressCN = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentFile", x => x.Id);
                });
        }
    }
}
