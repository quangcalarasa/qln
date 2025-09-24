using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class platformtdcup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "PlatformTdcs");

            migrationBuilder.AddColumn<double>(
                name: "LengthArea",
                table: "PlatformTdcs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Platcount",
                table: "PlatformTdcs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WidthArea",
                table: "PlatformTdcs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TdcPlatformManagers",
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
                    LandId = table.Column<int>(nullable: false),
                    TdcProjectId = table.Column<int>(nullable: false),
                    PlatformTdcId = table.Column<int>(nullable: false),
                    DistrictId = table.Column<int>(nullable: false),
                    TypeDecisionId = table.Column<int>(nullable: false),
                    TypeLegalId = table.Column<int>(nullable: false),
                    TdcPlatformArea = table.Column<decimal>(nullable: false),
                    Qantity = table.Column<int>(nullable: false),
                    ReceptionDate = table.Column<DateTime>(nullable: false),
                    ReceptionTime = table.Column<int>(nullable: true),
                    HandoverYear = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPlatformManagers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "LengthArea",
                table: "PlatformTdcs");

            migrationBuilder.DropColumn(
                name: "Platcount",
                table: "PlatformTdcs");

            migrationBuilder.DropColumn(
                name: "WidthArea",
                table: "PlatformTdcs");

            migrationBuilder.AddColumn<int>(
                name: "RoomNumber",
                table: "PlatformTdcs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
