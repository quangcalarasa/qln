using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class fixhandover : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandoverYear",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "HandoverYear",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<string>(
                name: "NoteHandoverCenter",
                table: "TdcApartmentManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteHandoverOther",
                table: "TdcApartmentManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoteHandoverPublic",
                table: "TdcApartmentManagers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OverYear",
                table: "TdcApartmentManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteHandoverCenter",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "NoteHandoverOther",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "NoteHandoverPublic",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "OverYear",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<int>(
                name: "HandoverYear",
                table: "TdcPlatformManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HandoverYear",
                table: "TdcApartmentManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
