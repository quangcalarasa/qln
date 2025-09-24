using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class xoadetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TdcPriceOneSellDetail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
