using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblblock_add_field_ExceedingLimitDeep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Width",
                table: "Block",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Deep",
                table: "Block",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExceedingLimitDeep",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExceedingLimitDeep",
                table: "Block");

            migrationBuilder.AlterColumn<float>(
                name: "Width",
                table: "Block",
                type: "real",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Deep",
                table: "Block",
                type: "real",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
