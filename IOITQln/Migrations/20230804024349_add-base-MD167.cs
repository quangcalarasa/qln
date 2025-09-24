using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addbaseMD167 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvServiceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    ServiceId = table.Column<int>(nullable: false),
                    FromDate = table.Column<DateTime>(nullable: false),
                    ToDate = table.Column<DateTime>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false),
                    TowerId = table.Column<int>(nullable: false),
                    ZoneId = table.Column<int>(nullable: false),
                    ServiceGroupId = table.Column<int>(nullable: false),
                    ServiceType = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 1024, nullable: true, defaultValue: ""),
                    TypeServiceConfig = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvServiceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Contract",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    ContractTypeId = table.Column<int>(nullable: false),
                    ContractGroupId = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    CustomerCode = table.Column<string>(nullable: true),
                    CustomerName = table.Column<string>(maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(maxLength: 20, nullable: false),
                    FromDate = table.Column<DateTime>(nullable: false),
                    ToDate = table.Column<DateTime>(nullable: false),
                    Note = table.Column<string>(maxLength: 1024, nullable: true),
                    ProjectId = table.Column<int>(nullable: false),
                    TowerId = table.Column<int>(nullable: false),
                    FloorId = table.Column<int>(nullable: false),
                    ApartmentId = table.Column<int>(nullable: false),
                    ApartmentInfo = table.Column<string>(maxLength: 51, nullable: true),
                    Receiver = table.Column<string>(maxLength: 200, nullable: true),
                    ReceiverAddress = table.Column<string>(maxLength: 512, nullable: true),
                    ReceiverPhone = table.Column<string>(maxLength: 20, nullable: true),
                    ContractStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Contract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ContractGroup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Code = table.Column<string>(maxLength: 100, nullable: false),
                    State = table.Column<bool>(nullable: false),
                    ContractTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ContractGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ContractType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    State = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ContractType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Customer",
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
                    CustomerType = table.Column<int>(nullable: false),
                    CustomerGroupId = table.Column<int>(maxLength: 1000, nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Phone = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AccountCode = table.Column<int>(nullable: false),
                    NationalId = table.Column<string>(nullable: true),
                    State = table.Column<bool>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167CustomerGroup",
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
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Code = table.Column<string>(maxLength: 1000, nullable: true),
                    State = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167CustomerGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Service",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Name = table.Column<string>(maxLength: 512, nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    State = table.Column<bool>(nullable: false),
                    ServiceGroupId = table.Column<int>(nullable: false),
                    serviceType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ServiceConfigDetail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    ServiceConfigId = table.Column<int>(nullable: false),
                    IsOnePrice = table.Column<bool>(nullable: false),
                    Des = table.Column<string>(maxLength: 512, nullable: false),
                    Quota = table.Column<int>(nullable: false),
                    Price = table.Column<int>(nullable: false),
                    SurchargePer = table.Column<double>(nullable: false),
                    SurchargeValue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ServiceConfigDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ServiceGroup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Name = table.Column<string>(maxLength: 512, nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    State = table.Column<bool>(nullable: false),
                    serviceType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ServiceGroup", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvServiceConfigs");

            migrationBuilder.DropTable(
                name: "Md167Contract");

            migrationBuilder.DropTable(
                name: "Md167ContractGroup");

            migrationBuilder.DropTable(
                name: "Md167ContractType");

            migrationBuilder.DropTable(
                name: "Md167Customer");

            migrationBuilder.DropTable(
                name: "Md167CustomerGroup");

            migrationBuilder.DropTable(
                name: "Md167Service");

            migrationBuilder.DropTable(
                name: "Md167ServiceConfigDetail");

            migrationBuilder.DropTable(
                name: "Md167ServiceGroup");
        }
    }
}
