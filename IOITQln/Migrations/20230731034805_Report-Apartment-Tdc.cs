using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class ReportApartmentTdc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TdcApartmentManagers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    BlockHouseId = table.Column<int>(nullable: false),
                    LandId = table.Column<int>(nullable: false),
                    FloorTdcId = table.Column<int>(nullable: false),
                    TdcProjectId = table.Column<int>(nullable: false),
                    ApartmentTdcId = table.Column<int>(nullable: false),
                    DistrictId = table.Column<int>(nullable: false),
                    TypeDecisionId = table.Column<int>(nullable: false),
                    Qantity = table.Column<int>(nullable: false),
                    ReceptionDate = table.Column<DateTime>(nullable: false),
                    Received = table.Column<bool>(nullable: false),
                    ReceivedYet = table.Column<bool>(nullable: false),
                    NotReceived = table.Column<bool>(nullable: false),
                    ReasonNotReceived = table.Column<string>(nullable: true),
                    HandoverYear = table.Column<string>(nullable: true),
                    HandoverNumber = table.Column<bool>(nullable: false),
                    HandoverPublic = table.Column<bool>(nullable: false),
                    HandoverOther = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdcApartmentManagers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcApartmentManagers");
        }
    }
}
