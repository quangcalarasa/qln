using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class changeIdRentFilev2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentFile",
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
                    Month = table.Column<int>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    FileStatus = table.Column<int>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    CodeHS = table.Column<string>(nullable: true),
                    DateHD = table.Column<DateTime>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    RentApartmentId = table.Column<int>(nullable: false),
                    RentBlockId = table.Column<int>(nullable: false),
                    Dob = table.Column<DateTime>(nullable: true),
                    AddressKH = table.Column<string>(nullable: true),
                    CodeKH = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    TypeReportApply = table.Column<int>(nullable: false),
                    TypeBlockId = table.Column<int>(nullable: false),
                    CampusArea = table.Column<float>(nullable: true),
                    fullAddressCN = table.Column<string>(nullable: true),
                    ConstructionAreaValue = table.Column<float>(nullable: false),
                    UseAreaValueCN = table.Column<float>(nullable: false),
                    CodeCN = table.Column<string>(nullable: true),
                    CodeCH = table.Column<string>(nullable: true),
                    UseAreaValueCH = table.Column<float>(nullable: false),
                    fullAddressCH = table.Column<string>(nullable: true),
                    DistrictId = table.Column<int>(nullable: false),
                    UsageStatus = table.Column<int>(nullable: true),
                    TypeHouse = table.Column<int>(nullable: false),
                    ProcessProfileCeCode = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    WardId = table.Column<int>(nullable: false),
                    LaneId = table.Column<int>(nullable: false),
                    proviceId = table.Column<int>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentFile", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentFile");
        }
    }
}
