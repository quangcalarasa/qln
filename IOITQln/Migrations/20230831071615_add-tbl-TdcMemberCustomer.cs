using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblTdcMemberCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlockHouseId",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FloorTdcId",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LandId",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TdcApartmentId",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TdcProjectId",
                table: "TdcCustomer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TdcMemberCustomer",
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
                    TdcCustomerId = table.Column<int>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    CCCD = table.Column<string>(nullable: true),
                    Dob = table.Column<DateTime>(nullable: false),
                    Phone = table.Column<string>(nullable: true),
                    AddressTt = table.Column<string>(nullable: true),
                    AddressLh = table.Column<string>(nullable: true),
                    LaneTt = table.Column<int>(nullable: true),
                    WardTt = table.Column<int>(nullable: false),
                    DistrictTt = table.Column<int>(nullable: false),
                    ProvinceTt = table.Column<int>(nullable: false),
                    LaneLh = table.Column<int>(nullable: true),
                    WardLh = table.Column<int>(nullable: false),
                    DistrictLh = table.Column<int>(nullable: false),
                    ProvinceLh = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcMemberCustomer", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcMemberCustomer");

            migrationBuilder.DropColumn(
                name: "BlockHouseId",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "FloorTdcId",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "LandId",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "TdcApartmentId",
                table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "TdcProjectId",
                table: "TdcCustomer");
        }
    }
}
