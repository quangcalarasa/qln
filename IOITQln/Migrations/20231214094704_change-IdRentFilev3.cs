using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class changeIdRentFilev3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "RentFileId",
                table: "RentFileBCT"
             );

            migrationBuilder.AddColumn<Guid>(
                name: "RentFileId",
                table: "RentFileBCT",
                nullable: false,
                type: "uniqueidentifier"
                );


            migrationBuilder.DropColumn(
                name: "RentFileId",
                table: "RentBctTable"
             );

            migrationBuilder.AddColumn<Guid>(
                name: "RentFileId",
                table: "RentBctTable",
                nullable: false,
                type: "uniqueidentifier"
                );

            migrationBuilder.DropColumn(
              name: "RentFileId",
              table: "MemberRentFile"
           );

            migrationBuilder.AddColumn<Guid>(
                name: "RentFileId",
                table: "MemberRentFile",
                nullable: false,
                type: "uniqueidentifier"
                );

            migrationBuilder.DropColumn(
            name: "RentFileId",
            table: "DebtsTable"
         );

            migrationBuilder.AddColumn<Guid>(
                name: "RentFileId",
                table: "DebtsTable",
                nullable: false,
                type: "uniqueidentifier"
                );

            migrationBuilder.DropColumn(
            name: "RentFileId",
            table: "Debts"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "RentFileId",
                table: "Debts",
                nullable: false,
                type: "uniqueidentifier"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RentFileId",
                table: "RentFileBCT",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<int>(
                name: "RentFileId",
                table: "RentBctTable",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RentFileId",
                table: "MemberRentFile",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<int>(
                name: "RentFileId",
                table: "DebtsTable",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<int>(
                name: "RentFileId",
                table: "Debts",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid));
        }
    }
}
