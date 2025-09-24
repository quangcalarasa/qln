using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace IOITQln.Migrations
{
    public partial class gia1lan1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TdcPriceOneTime",
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
                    Customer = table.Column<string>(nullable: true),
                    Code = table.Column<string>(maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Floor1 = table.Column<int>(nullable: false),
                    TdcProjectId = table.Column<int>(nullable: false),
                    TdcLandId = table.Column<int>(nullable: false),
                    TdcBlockHouseId = table.Column<int>(nullable: false),
                    TdcFloorTdcId = table.Column<int>(nullable: false),
                    TdcApartmentId = table.Column<int>(nullable: false),
                    TdcPlatformId = table.Column<int>(nullable: false),
                    PesonalTax = table.Column<decimal>(nullable: false),
                    RegistrationTax = table.Column<decimal>(nullable: false),
                    DecisionNumberCT = table.Column<int>(nullable: false),
                    DecisionDateCT = table.Column<DateTime>(nullable: false),
                    DecisionNumberTT = table.Column<int>(nullable: false),
                    DecisionDateTT = table.Column<DateTime>(nullable: false),
                    TotalAreaTT = table.Column<decimal>(nullable: false),
                    TotalAreaCT = table.Column<decimal>(nullable: false),
                    TotalPriceTT = table.Column<decimal>(nullable: false),
                    TotalPriceCT = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceOneTime", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
