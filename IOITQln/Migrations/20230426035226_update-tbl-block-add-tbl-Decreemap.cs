using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class updatetblblockaddtblDecreemap : Migration
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

            //migrationBuilder.DropColumn(
            //    name: "TdcCustomerId",
            //    table: "TDCProjectIngrePrice");

            //migrationBuilder.DropColumn(
            //    name: "TotalApartment",
            //    table: "TdcCustomer");

            //migrationBuilder.DropColumn(
            //    name: "TotalAreas",
            //    table: "TdcCustomer");

            //migrationBuilder.DropColumn(
            //    name: "TotalBuildAreas",
            //    table: "TdcCustomer");

            //migrationBuilder.DropColumn(
            //    name: "TotalFloorAreas",
            //    table: "TdcCustomer");

            //migrationBuilder.DropColumn(
            //    name: "TotalPlatform",
            //    table: "TdcCustomer");

            //migrationBuilder.DropColumn(
            //    name: "TotalUseAreas",
            //    table: "TdcCustomer");

            //migrationBuilder.DropColumn(
            //    name: "Ward",
            //    table: "TdcCustomer");

            migrationBuilder.DropColumn(
                name: "IsAlley",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemId",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemValue",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocation",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocationInAlley",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LevelAlley",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientId",
                table: "Block");

            migrationBuilder.AlterColumn<string>(
                name: "TextBasedInfo",
                table: "Block",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Block",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MapNo",
                table: "Block",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "LandNo",
                table: "Block",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Block",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AlleyLandScapePrice_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlleyPositionCoefficientId_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlleyPositionCoefficientStr_34",
                table: "Block",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAlley_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandPriceItemId_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandPriceItemId_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandPriceItemId_99",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LandPriceItemValue_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LandPriceItemValue_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LandPriceItemValue_99",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandPriceRefinement_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandPriceRefinement_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LandPriceRefinement_99",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LandScapePrice_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LandScapePrice_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LandScapePrice_99",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocationInAlley_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocation_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocation_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocation_99",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LevelAlley_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionCoefficientId_34",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionCoefficientId_61",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionCoefficientId_99",
                table: "Block",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionCoefficientStr_34",
                table: "Block",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionCoefficientStr_61",
                table: "Block",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionCoefficientStr_99",
                table: "Block",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DecreeMap",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    CreatedById = table.Column<long>(nullable: true),
                    UpdatedById = table.Column<long>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    TargetId = table.Column<long>(nullable: false),
                    DecreeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecreeMap", x => x.Id);
                });

            //migrationBuilder.CreateTable(
            //    name: "TdcCustomerFile",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Status = table.Column<int>(nullable: false, defaultValue: 1),
            //        CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
            //        UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
            //        CreatedById = table.Column<long>(nullable: true),
            //        UpdatedById = table.Column<long>(nullable: true),
            //        CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
            //        UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
            //        TdcCustomerId = table.Column<int>(nullable: false),
            //        FileName = table.Column<string>(nullable: true),
            //        Note = table.Column<string>(nullable: true),
            //        File = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TdcCustomerFile", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TDCProject",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Status = table.Column<int>(nullable: false, defaultValue: 1),
            //        CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
            //        UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
            //        CreatedById = table.Column<long>(nullable: true),
            //        UpdatedById = table.Column<long>(nullable: true),
            //        CreatedBy = table.Column<string>(maxLength: 500, nullable: true),
            //        UpdatedBy = table.Column<string>(maxLength: 500, nullable: true),
            //        Code = table.Column<string>(maxLength: 1000, nullable: true),
            //        Name = table.Column<string>(maxLength: 2000, nullable: false),
            //        LandCount = table.Column<int>(nullable: false),
            //        FullAddress = table.Column<string>(nullable: true),
            //        HouseNumber = table.Column<string>(nullable: false),
            //        Lane = table.Column<int>(nullable: true),
            //        Ward = table.Column<int>(nullable: false),
            //        District = table.Column<int>(nullable: false),
            //        Province = table.Column<int>(nullable: false),
            //        BuildingName = table.Column<string>(nullable: true),
            //        TotalAreas = table.Column<double>(nullable: false),
            //        TotalApartment = table.Column<int>(nullable: false),
            //        TotalPlatform = table.Column<int>(nullable: false),
            //        TotalFloorAreas = table.Column<double>(nullable: false),
            //        TotalUseAreas = table.Column<double>(nullable: false),
            //        TotalBuildAreas = table.Column<double>(nullable: false),
            //        Note = table.Column<string>(maxLength: 4000, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TDCProject", x => x.Id);
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecreeMap");

            //migrationBuilder.DropTable(
            //    name: "TdcCustomerFile");

            //migrationBuilder.DropTable(
            //    name: "TDCProject");

            migrationBuilder.DropColumn(
                name: "AlleyLandScapePrice_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "AlleyPositionCoefficientId_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "AlleyPositionCoefficientStr_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "IsAlley_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemId_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemId_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemId_99",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemValue_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemValue_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceItemValue_99",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceRefinement_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceRefinement_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandPriceRefinement_99",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandScapePrice_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandScapePrice_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandScapePrice_99",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocationInAlley_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocation_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocation_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LandscapeLocation_99",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "LevelAlley_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientId_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientId_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientId_99",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientStr_34",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientStr_61",
                table: "Block");

            migrationBuilder.DropColumn(
                name: "PositionCoefficientStr_99",
                table: "Block");

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

            //migrationBuilder.AddColumn<int>(
            //    name: "TdcCustomerId",
            //    table: "TDCProjectIngrePrice",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "TotalApartment",
            //    table: "TdcCustomer",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<double>(
            //    name: "TotalAreas",
            //    table: "TdcCustomer",
            //    type: "float",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "TotalBuildAreas",
            //    table: "TdcCustomer",
            //    type: "float",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<double>(
            //    name: "TotalFloorAreas",
            //    table: "TdcCustomer",
            //    type: "float",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<int>(
            //    name: "TotalPlatform",
            //    table: "TdcCustomer",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<double>(
            //    name: "TotalUseAreas",
            //    table: "TdcCustomer",
            //    type: "float",
            //    nullable: false,
            //    defaultValue: 0.0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Ward",
            //    table: "TdcCustomer",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TextBasedInfo",
                table: "Block",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Block",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MapNo",
                table: "Block",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "LandNo",
                table: "Block",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Block",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAlley",
                table: "Block",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandPriceItemId",
                table: "Block",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LandPriceItemValue",
                table: "Block",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocation",
                table: "Block",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandscapeLocationInAlley",
                table: "Block",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LevelAlley",
                table: "Block",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionCoefficientId",
                table: "Block",
                type: "int",
                nullable: true);
        }
    }
}
