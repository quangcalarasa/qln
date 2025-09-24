using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblmd167tranferunit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferUnit",
                table: "Md167House");

            migrationBuilder.AddColumn<int>(
                name: "Md167TransferUnitId",
                table: "Md167House",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Md167TranferUnit",
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
                    Code = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Adress = table.Column<string>(maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167TranferUnit", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Md167TranferUnit");

            migrationBuilder.DropColumn(
                name: "Md167TransferUnitId",
                table: "Md167House");

            migrationBuilder.AddColumn<string>(
                name: "TransferUnit",
                table: "Md167House",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
