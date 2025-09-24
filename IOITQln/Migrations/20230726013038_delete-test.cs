using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOITQln.Migrations
{
    public partial class deletetest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TDCprojectests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
        }
    }
}
