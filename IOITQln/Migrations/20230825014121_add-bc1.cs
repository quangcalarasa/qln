using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addbc1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HandOverCenter",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HandOverOther",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HandoverPublic",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonReceivedYet",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reminded",
                table: "TdcPlatformManagers",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HandoverPublic",
                table: "TdcApartmentManagers",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "HandoverOther",
                table: "TdcApartmentManagers",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "HandOverCenter",
                table: "TdcApartmentManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonReceivedYet",
                table: "TdcApartmentManagers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reminded",
                table: "TdcApartmentManagers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandOverCenter",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "HandOverOther",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "HandoverPublic",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "ReasonReceivedYet",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "Reminded",
                table: "TdcPlatformManagers");

            migrationBuilder.DropColumn(
                name: "HandOverCenter",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "ReasonReceivedYet",
                table: "TdcApartmentManagers");

            migrationBuilder.DropColumn(
                name: "Reminded",
                table: "TdcApartmentManagers");

            migrationBuilder.AlterColumn<bool>(
                name: "HandoverPublic",
                table: "TdcApartmentManagers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HandoverOther",
                table: "TdcApartmentManagers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }
    }
}
