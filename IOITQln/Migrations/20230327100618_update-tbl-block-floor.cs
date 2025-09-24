using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblblockfloor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Code",
                table: "Floor",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "UseAreaNote",
                table: "Block",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "ConstructionAreaNote",
                table: "Block",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AddColumn<int>(
                name: "DecreeType1Id",
                table: "Block",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HighwayPlanningInfo",
                table: "Block",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandAcquisitionSituationInfo",
                table: "Block",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LandAreaSpecial",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandAreaSpecialS1",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandAreaSpecialS2",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandAreaSpecialS3",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandPositionSpecial",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LandSpecial",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandUsePlanningInfo",
                table: "Block",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionCoefficientId",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceListId",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PriceListValue",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SpecialCase",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextBasedInfo",
                table: "Block",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WidthSpecial",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WidthSpecialS1",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WidthSpecialS2",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WidthSpecialS3",
                table: "Block",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeType1Id",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "HighwayPlanningInfo",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandAcquisitionSituationInfo",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandAreaSpecial",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandAreaSpecialS1",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandAreaSpecialS2",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandAreaSpecialS3",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPositionSpecial",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandSpecial",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandUsePlanningInfo",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PriceListValue",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "SpecialCase",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "TextBasedInfo",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "WidthSpecial",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "WidthSpecialS1",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "WidthSpecialS2",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "WidthSpecialS3",
                table: "Block");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Floor",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "UseAreaNote",
                table: "Block",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConstructionAreaNote",
                table: "Block",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
