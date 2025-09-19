using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Update_column_table_TA_Rules_Global : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NightShiftEndTime",
                table: "TA_Rules_Global",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NightShiftOvernightEndTime",
                table: "TA_Rules_Global",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NightShiftStartTime",
                table: "TA_Rules_Global",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NightShiftEndTime",
                table: "TA_Rules_Global");

            migrationBuilder.DropColumn(
                name: "NightShiftOvernightEndTime",
                table: "TA_Rules_Global");

            migrationBuilder.DropColumn(
                name: "NightShiftStartTime",
                table: "TA_Rules_Global");
        }
    }
}
