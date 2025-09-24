using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblTdcCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TdcAuthCustomerDetail",
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
                    Dob = table.Column<DateTime>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    AddressTT = table.Column<string>(nullable: true),
                    AddressLT = table.Column<string>(nullable: true),
                    AreaName = table.Column<string>(nullable: true),
                    Lane = table.Column<int>(nullable: true),
                    Ward = table.Column<int>(nullable: false),
                    District = table.Column<int>(nullable: false),
                    Province = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcAuthCustomerDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TdcCustomer",
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
                    Code = table.Column<string>(maxLength: 500, nullable: true),
                    FullName = table.Column<string>(maxLength: 500, nullable: false),
                    Dob = table.Column<DateTime>(nullable: true),
                    Phone = table.Column<string>(maxLength: 100, nullable: true),
                    Email = table.Column<string>(maxLength: 200, nullable: true),
                    AddressTT = table.Column<string>(maxLength: 2000, nullable: true),
                    AddressLT = table.Column<string>(maxLength: 2000, nullable: true),
                    AreaName = table.Column<string>(nullable: true),
                    Lane = table.Column<int>(nullable: true),
                    Ward = table.Column<int>(nullable: false),
                    District = table.Column<int>(nullable: false),
                    Province = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 2000, nullable: true),
                    CCCD = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TdcCustomerFile",
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
                    FileName = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcCustomerFile", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcAuthCustomerDetail");

            migrationBuilder.DropTable(
                name: "TdcCustomer");

            migrationBuilder.DropTable(
                name: "TdcCustomerFile");
        }
    }
}
