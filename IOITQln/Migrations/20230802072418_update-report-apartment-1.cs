using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatereportapartment1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandoverNumber",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "NotReceived",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "ReasonNotReceived",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "Received",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "ReceivedYet",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<int>(
                name: "ReceptionTime",
                table: "TdcApartmentManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceptionTime",
                table: "TdcApartmentManagers");

            migrationBuilder.AddColumn<bool>(
                name: "HandoverNumber",
                table: "TdcApartmentManagers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotReceived",
                table: "TdcApartmentManagers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReasonNotReceived",
                table: "TdcApartmentManagers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Received",
                table: "TdcApartmentManagers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReceivedYet",
                table: "TdcApartmentManagers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
