using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtblPriceRent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "File",
            //    table: "TDCProjectIngrePrice");

            //migrationBuilder.DropColumn(
            //    name: "FileName",
            //    table: "TDCProjectIngrePrice");

            //migrationBuilder.DropColumn(
            //    name: "Note",
            //    table: "TDCProjectIngrePrice");

            migrationBuilder.CreateTable(
                name: "TdcPriceRent",
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
                    DateTDC = table.Column<DateTime>(nullable: false),
                    MonthRent = table.Column<int>(nullable: false),
                    PriceTC = table.Column<decimal>(nullable: false),
                    PriceMonth = table.Column<decimal>(nullable: false),
                    PriceToTal = table.Column<decimal>(nullable: false),
                    PriceTT = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcPriceRent", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcPriceRent");

            //migrationBuilder.AddColumn<string>(
            //    name: "File",
            //    table: "TDCProjectIngrePrice",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "FileName",
            //    table: "TDCProjectIngrePrice",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Note",
            //    table: "TDCProjectIngrePrice",
            //    type: "nvarchar(max)",
            //    nullable: true);
        }
    }
}
