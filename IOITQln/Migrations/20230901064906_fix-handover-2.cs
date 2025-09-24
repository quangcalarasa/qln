using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixhandover2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HandOverYear",
                table: "TdcPlatformManagers",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "NoteHandoverCenter",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteHandoverOther",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteHandoverPublic",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HandOverYear",
                table: "TdcApartmentManagers",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteHandoverCenter",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "NoteHandoverOther",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "NoteHandoverPublic",
                table: "TdcPlatformManagers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HandOverYear",
                table: "TdcPlatformManagers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "HandOverYear",
                table: "TdcApartmentManagers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
