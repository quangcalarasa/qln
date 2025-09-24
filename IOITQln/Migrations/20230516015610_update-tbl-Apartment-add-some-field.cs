using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblApartmentaddsomefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Blueprint",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CampusArea",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Dispute",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstablishStateOwnership",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeApartmentEntity",
                table: "Apartment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsageStatus",
                table: "Apartment",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageStatusNote",
                table: "Apartment",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UseAreaNote1",
                table: "Apartment",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Blueprint",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "CampusArea",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "Dispute",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "EstablishStateOwnership",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "TypeApartmentEntity",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "UsageStatus",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "UsageStatusNote",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "UseAreaNote1",
                table: "Apartment");
        }
    }
}
