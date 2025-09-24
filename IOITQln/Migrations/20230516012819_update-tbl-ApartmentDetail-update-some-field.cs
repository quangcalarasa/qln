using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblApartmentDetailupdatesomefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralAreaValue",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "LevelApartment",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "PersonalAreaValue",
                table: "ApartmentDetail");

            migrationBuilder.AlterColumn<int>(
                name: "FloorApplyCoefficient",
                table: "ApartmentDetail",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DecreeType1Id",
                table: "ApartmentDetail",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<float>(
                name: "GeneralArea",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PrivateArea",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalAreaDetailFloor",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalAreaFloor",
                table: "ApartmentDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "YardArea",
                table: "ApartmentDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneralArea",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "PrivateArea",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "TotalAreaDetailFloor",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "TotalAreaFloor",
                table: "ApartmentDetail");

            migrationBuilder.DropColumn(
                name: "YardArea",
                table: "ApartmentDetail");

            migrationBuilder.AlterColumn<int>(
                name: "FloorApplyCoefficient",
                table: "ApartmentDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DecreeType1Id",
                table: "ApartmentDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "GeneralAreaValue",
                table: "ApartmentDetail",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LevelApartment",
                table: "ApartmentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "PersonalAreaValue",
                table: "ApartmentDetail",
                type: "real",
                nullable: true);
        }
    }
}
