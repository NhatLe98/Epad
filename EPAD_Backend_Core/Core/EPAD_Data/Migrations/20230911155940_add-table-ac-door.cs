using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addtableacdoor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyIndex",
                table: "AC_TimeZone",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AC_TimeZone",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyIndex",
                table: "AC_AccHoliday",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HolidayName",
                table: "AC_AccHoliday",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AC_AccHoliday",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "AC_AccHoliday",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyIndex",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AC_AccGroup",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUser",
                table: "AC_AccGroup",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyIndex",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "CompanyIndex",
                table: "AC_AccHoliday");

            migrationBuilder.DropColumn(
                name: "HolidayName",
                table: "AC_AccHoliday");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AC_AccHoliday");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "AC_AccHoliday");

            migrationBuilder.DropColumn(
                name: "CompanyIndex",
                table: "AC_AccGroup");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AC_AccGroup");

            migrationBuilder.DropColumn(
                name: "UpdatedUser",
                table: "AC_AccGroup");
        }
    }
}
