using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblcustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
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
                    Address = table.Column<string>(maxLength: 2000, nullable: true),
                    Avatar = table.Column<string>(maxLength: 1000, nullable: true),
                    Sex = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeductionLandMoney",
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
                    DecreeType1Id = table.Column<int>(nullable: true),
                    DecreeType2Id = table.Column<int>(nullable: false),
                    Condition = table.Column<string>(maxLength: 4000, nullable: true),
                    Note = table.Column<string>(maxLength: 4000, nullable: true),
                    Value = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeductionLandMoney", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributionFloorCoefficient",
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
                    DecreeType1Id = table.Column<int>(nullable: false),
                    DecreeType2Id = table.Column<int>(nullable: true),
                    ApplyMezzanineCoefficient = table.Column<bool>(nullable: true),
                    MezzanineCoefficient = table.Column<float>(nullable: true),
                    FlatCoefficient = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionFloorCoefficient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributionFloorCoefficientDetail",
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
                    DistributionFloorCoefficientId = table.Column<int>(nullable: false),
                    TypeFloor = table.Column<int>(nullable: false),
                    NumberFloor = table.Column<int>(nullable: false),
                    Value = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionFloorCoefficientDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandCorrectionCoefficient",
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
                    DecreeType1Id = table.Column<int>(nullable: false),
                    DecreeType2Id = table.Column<int>(nullable: true),
                    Code = table.Column<string>(maxLength: 1000, nullable: true),
                    Name = table.Column<string>(maxLength: 2000, nullable: true),
                    Note = table.Column<string>(maxLength: 4000, nullable: true),
                    Value = table.Column<float>(nullable: false),
                    AlleyWidth = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandCorrectionCoefficient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PositionCoefficient",
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
                    DecreeType1Id = table.Column<int>(nullable: false),
                    DecreeType2Id = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 2000, nullable: true),
                    DoApply = table.Column<DateTime>(nullable: false),
                    LocationValue1 = table.Column<float>(nullable: false),
                    LocationValue2 = table.Column<float>(nullable: false),
                    LocationValue3 = table.Column<float>(nullable: false),
                    LocationValue4 = table.Column<float>(nullable: false),
                    AlleyValue1 = table.Column<float>(nullable: false),
                    AlleyValue2 = table.Column<float>(nullable: false),
                    AlleyValue3 = table.Column<float>(nullable: false),
                    AlleyValue4 = table.Column<float>(nullable: false),
                    AlleyLevel2 = table.Column<float>(nullable: true),
                    AlleyOther = table.Column<float>(nullable: true),
                    AlleyLand = table.Column<float>(nullable: true),
                    PositionDeep = table.Column<float>(nullable: true),
                    LandPriceRefinement = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionCoefficient", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "DeductionLandMoney");

            migrationBuilder.DropTable(
                name: "DistributionFloorCoefficient");

            migrationBuilder.DropTable(
                name: "DistributionFloorCoefficientDetail");

            migrationBuilder.DropTable(
                name: "LandCorrectionCoefficient");

            migrationBuilder.DropTable(
                name: "PositionCoefficient");
        }
    }
}
