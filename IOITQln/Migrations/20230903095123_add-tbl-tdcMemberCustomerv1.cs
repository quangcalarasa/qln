using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class addtbltdcMemberCustomerv1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealityNumber",
                table: "TdcPlatformManagers");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "TdcMemberCustomer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "TdcMemberCustomer");

            migrationBuilder.AddColumn<int>(
                name: "RealityNumber",
                table: "TdcPlatformManagers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
