using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblupdateDatabaseRenFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "IssuedBy",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "LaneId",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "RentFile");

            migrationBuilder.AddColumn<float>(
                name: "CampusArea",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCH",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeKH",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ConstructionAreaValue",
                table: "RentFile",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateHD",
                table: "RentFile",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Dob",
                table: "RentFile",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "RentFile",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeBlockId",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeReportApply",
                table: "RentFile",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "UseAreaValue",
                table: "RentFile",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "fullAddressCH",
                table: "RentFile",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MemberRentFile",
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
                    RentFileId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Relationship = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRentFile", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberRentFile");

            migrationBuilder.DropColumn(
                name: "CampusArea",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "CodeCH",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "CodeKH",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "ConstructionAreaValue",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "DateHD",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "Dob",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "TypeBlockId",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "TypeReportApply",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "UseAreaValue",
                table: "RentFile");

            migrationBuilder.DropColumn(
                name: "fullAddressCH",
                table: "RentFile");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "RentFile",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "RentFile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                table: "RentFile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LaneId",
                table: "RentFile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "RentFile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardId",
                table: "RentFile",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
