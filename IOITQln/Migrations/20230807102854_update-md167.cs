using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatemd167 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "LandPriceType",
                table: "LandPrice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Md167AreaValue",
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
                    Value = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Decision = table.Column<string>(maxLength: 1000, nullable: true),
                    EffectiveTime = table.Column<DateTime>(nullable: false),
                    LandPurpose = table.Column<string>(maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167AreaValue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Auctioneer",
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
                    Code = table.Column<string>(maxLength: 1000, nullable: true),
                    UnitName = table.Column<string>(nullable: true),
                    UnitAddress = table.Column<string>(maxLength: 1000, nullable: true),
                    TaxNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    BusinessLicense = table.Column<string>(maxLength: 1000, nullable: true),
                    RepresentFullName = table.Column<string>(maxLength: 1000, nullable: true),
                    RepresentPosition = table.Column<string>(maxLength: 1000, nullable: true),
                    RepresentIDCard = table.Column<string>(maxLength: 1000, nullable: true),
                    RepresentDateOfIssue = table.Column<DateTime>(nullable: false),
                    RepresentPlaceOfIssue = table.Column<string>(maxLength: 1000, nullable: true),
                    ContactAddress = table.Column<string>(maxLength: 1000, nullable: true),
                    ContactPhoneNumber = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Auctioneer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Delegate",
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
                    Code = table.Column<string>(maxLength: 1000, nullable: true),
                    PersonOrCompany = table.Column<int>(nullable: false),
                    PerName = table.Column<string>(maxLength: 1000, nullable: true),
                    PerNationalId = table.Column<string>(maxLength: 1000, nullable: true),
                    PerDateOfIssue = table.Column<DateTime>(nullable: true),
                    PerPlaceOfIssue = table.Column<string>(maxLength: 1000, nullable: true),
                    PerAddress = table.Column<string>(maxLength: 1000, nullable: true),
                    PerPhoneNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    ComName = table.Column<string>(maxLength: 1000, nullable: true),
                    ComTaxNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    ComBusinessLicense = table.Column<string>(nullable: true),
                    ComOrganizationRepresentativeName = table.Column<string>(maxLength: 1000, nullable: true),
                    ComPosition = table.Column<string>(maxLength: 1000, nullable: true),
                    ComIDCard = table.Column<string>(maxLength: 1000, nullable: true),
                    ComDateOfIssue = table.Column<DateTime>(nullable: true),
                    ComPlaceOfIssue = table.Column<string>(maxLength: 1000, nullable: true),
                    ComAddress = table.Column<string>(maxLength: 1000, nullable: true),
                    ComPhoneNumber = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Delegate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167House",
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
                    Code = table.Column<string>(maxLength: 1000, nullable: true),
                    HouseNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    ProvinceId = table.Column<int>(nullable: false),
                    DistrictId = table.Column<int>(nullable: false),
                    WardId = table.Column<int>(nullable: false),
                    LaneId = table.Column<int>(nullable: false),
                    MapNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    ParcelNumber = table.Column<string>(maxLength: 1000, nullable: true),
                    LandTaxRate = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PlanningInfor = table.Column<string>(maxLength: 1000, nullable: true),
                    LandId = table.Column<int>(nullable: false),
                    TransferUnit = table.Column<string>(maxLength: 1000, nullable: true),
                    Location = table.Column<string>(maxLength: 1000, nullable: true),
                    LocationCoefficient = table.Column<decimal>(nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SHNNCode = table.Column<string>(maxLength: 1000, nullable: true),
                    SHNNDate = table.Column<DateTime>(nullable: false),
                    ContractCode = table.Column<string>(maxLength: 1000, nullable: true),
                    ContractDate = table.Column<DateTime>(nullable: false),
                    LeaseCode = table.Column<string>(maxLength: 1000, nullable: true),
                    LeaseDate = table.Column<DateTime>(nullable: false),
                    LeaseCertCode = table.Column<string>(maxLength: 1000, nullable: true),
                    LeaseCertDate = table.Column<DateTime>(nullable: false),
                    Md167HouseId = table.Column<int>(nullable: true),
                    PurposeUsing = table.Column<int>(nullable: false),
                    DocumentCode = table.Column<string>(maxLength: 1000, nullable: true),
                    PlanContent = table.Column<int>(nullable: false),
                    OriginPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ValueLand = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TypeHouse = table.Column<int>(nullable: false),
                    StatusOfUse = table.Column<int>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    Info = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167House", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167HouseType",
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
                    Code = table.Column<string>(maxLength: 1000, nullable: false),
                    Name = table.Column<string>(maxLength: 2000, nullable: false),
                    Note = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167HouseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167LandTax",
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
                    Code = table.Column<string>(maxLength: 2000, nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167LandTax", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167PositionValue",
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
                    DoApply = table.Column<DateTime>(nullable: false),
                    Position1 = table.Column<string>(maxLength: 2000, nullable: false),
                    Position2 = table.Column<string>(maxLength: 2000, nullable: false),
                    Position3 = table.Column<string>(maxLength: 2000, nullable: false),
                    Position4 = table.Column<string>(maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167PositionValue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167VATValue",
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
                    Value = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(nullable: false),
                    Note = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167VATValue", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Md167AreaValue");

            migrationBuilder.DropTable(
                name: "Md167Auctioneer");

            migrationBuilder.DropTable(
                name: "Md167Delegate");

            migrationBuilder.DropTable(
                name: "Md167House");

            migrationBuilder.DropTable(
                name: "Md167HouseType");

            migrationBuilder.DropTable(
                name: "Md167LandTax");

            migrationBuilder.DropTable(
                name: "Md167PositionValue");

            migrationBuilder.DropTable(
                name: "Md167VATValue");

            migrationBuilder.DropColumn(
                name: "LandPriceType",
                table: "LandPrice");

            migrationBuilder.CreateTable(
                name: "InvServiceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true, defaultValue: ""),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ServiceGroupId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TowerId = table.Column<int>(type: "int", nullable: false),
                    TypeServiceConfig = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    ZoneId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvServiceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Contract",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ApartmentId = table.Column<int>(type: "int", nullable: false),
                    ApartmentInfo = table.Column<string>(type: "nvarchar(51)", maxLength: 51, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContractGroupId = table.Column<int>(type: "int", nullable: false),
                    ContractStatus = table.Column<int>(type: "int", nullable: false),
                    ContractTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FloorId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Receiver = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReceiverAddress = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ReceiverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TowerId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Contract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ContractGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContractTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ContractGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ContractType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ContractType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountCode = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CustomerGroupId = table.Column<int>(type: "int", maxLength: 1000, nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167CustomerGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167CustomerGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ServiceGroupId = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    serviceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167Service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ServiceConfigDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Des = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsOnePrice = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Quota = table.Column<int>(type: "int", nullable: false),
                    ServiceConfigId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SurchargePer = table.Column<double>(type: "float", nullable: false),
                    SurchargeValue = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ServiceConfigDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Md167ServiceGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedById = table.Column<long>(type: "bigint", nullable: true),
                    serviceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Md167ServiceGroup", x => x.Id);
                });
        }
    }
}
