using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_AttendanceLog_Add_New_Field_AVN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessDate",
                table: "IC_AttendanceLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTime",
                table: "IC_AttendanceLog",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessDate",
                table: "IC_AttendanceLog");

            migrationBuilder.DropColumn(
                name: "AccessTime",
                table: "IC_AttendanceLog");
        }
    }
}
