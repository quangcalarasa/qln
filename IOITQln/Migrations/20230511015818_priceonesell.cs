using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class priceonesell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcPriceOneTimeTemporary");

            migrationBuilder.DropTable(
                name: "TdcPriceOneTimeOfficial");

            migrationBuilder.DropTable(
                name: "TdcPriceOneTime");

            migrationBuilder.CreateTable(
                name: "TdcPriceOneSell",
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
                    Date = table.Column<DateTime>(nullable: false),
                    Floor1 = table.Column<string>(nullable: true),
                    TdcCustomerId = table.Column<int>(nullable: false),
                    TdcProjectId = table.Column<int>(nullable: false),
                    LandId = table.Column<int>(nullable: false),
                    BlockHouseId = table.Column<int>(nullable: false),
                    FloorTdcId = table.Column<int>(nullable: false),
                    PlatformId = table.Column<int>(nullable: false),
                    TdcApartmentId = table.Column<int>(nullable: false),
                    PersonalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DecisionNumberCT = table.Column<int>(nullable: false),
                    DecisionDateCT = table.Column<DateTime>(nullable: false),
                    DecisionNumberTT = table.Column<int>(nullable: false),
                    DecisionDateTT = table.Column<DateTime>(nullable: false),
                    TotalAreaTT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAreaCT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPriceTT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPriceCT = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceOneSell", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TdcPriceOneSellOfficial",
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
                    TdcPriceOneSellId = table.Column<int>(nullable: false),
                    IngredientsPriceId = table.Column<int>(nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceOneSellOfficial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TdcPriceOneSellTemporary",
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
                    TdcPriceOneSellId = table.Column<int>(nullable: false),
                    IngredientsPriceId = table.Column<int>(nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceOneSellTemporary", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
