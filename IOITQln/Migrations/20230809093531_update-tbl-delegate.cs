using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetbldelegate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComAddress",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "ComDateOfIssue",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "ComIDCard",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "ComName",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "ComPhoneNumber",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "ComPlaceOfIssue",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PerAddress",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PerDateOfIssue",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PerName",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PerNationalId",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PerPhoneNumber",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PerPlaceOfIssue",
                table: "Md167Delegate");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Md167Delegate",
                maxLength: 1000,
                nullable: true,
                defaultValue: "0",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Md167Delegate",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfIssue",
                table: "Md167Delegate",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Md167Delegate",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Md167Delegate",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Md167Delegate",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfIssue",
                table: "Md167Delegate",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "DateOfIssue",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Md167Delegate");

            migrationBuilder.DropColumn(
                name: "PlaceOfIssue",
                table: "Md167Delegate");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true,
                oldDefaultValue: "0");

            migrationBuilder.AddColumn<string>(
                name: "ComAddress",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComDateOfIssue",
                table: "Md167Delegate",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComIDCard",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComName",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComPhoneNumber",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComPlaceOfIssue",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerAddress",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PerDateOfIssue",
                table: "Md167Delegate",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerName",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerNationalId",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerPhoneNumber",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerPlaceOfIssue",
                table: "Md167Delegate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
