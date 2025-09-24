using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblBlockDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coefficient",
                table: "BlockDetail");

            migrationBuilder.AlterColumn<float>(
                name: "TotalAreaFloor",
                table: "BlockDetail",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "TotalAreaDetailFloor",
                table: "BlockDetail",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "PrivateArea",
                table: "BlockDetail",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "GeneralArea",
                table: "BlockDetail",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<float>(
                name: "Coefficient_34",
                table: "BlockDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Coefficient_61",
                table: "BlockDetail",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Coefficient_99",
                table: "BlockDetail",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coefficient_34",
                table: "BlockDetail");

            migrationBuilder.DropColumn(
                name: "Coefficient_61",
                table: "BlockDetail");

            migrationBuilder.DropColumn(
                name: "Coefficient_99",
                table: "BlockDetail");

            migrationBuilder.AlterColumn<float>(
                name: "TotalAreaFloor",
                table: "BlockDetail",
                type: "real",
                nullable: false,
                oldClrType: typeof(float),
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "TotalAreaDetailFloor",
                table: "BlockDetail",
                type: "real",
                nullable: false,
                oldClrType: typeof(float),
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "PrivateArea",
                table: "BlockDetail",
                type: "real",
                nullable: false,
                oldClrType: typeof(float),
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "GeneralArea",
                table: "BlockDetail",
                type: "real",
                nullable: false,
                oldClrType: typeof(float),
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Coefficient",
                table: "BlockDetail",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
