using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Rules_GeneralAccess_Add_New_Field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BeginLastHaftTime",
                table: "GC_Rules_GeneralAccess",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndFirstHaftTime",
                table: "GC_Rules_GeneralAccess",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeginLastHaftTime",
                table: "GC_Rules_GeneralAccess");

            migrationBuilder.DropColumn(
                name: "EndFirstHaftTime",
                table: "GC_Rules_GeneralAccess");
        }
    }
}
