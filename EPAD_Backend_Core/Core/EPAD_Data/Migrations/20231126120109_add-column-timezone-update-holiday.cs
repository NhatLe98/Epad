using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addcolumntimezoneupdateholiday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HolidayStart3",
                table: "AC_TimeZone",
                newName: "Holiday3Start3");

            migrationBuilder.RenameColumn(
                name: "HolidayStart2",
                table: "AC_TimeZone",
                newName: "Holiday3Start2");

            migrationBuilder.RenameColumn(
                name: "HolidayStart1",
                table: "AC_TimeZone",
                newName: "Holiday3Start1");

            migrationBuilder.RenameColumn(
                name: "HolidayEnd3",
                table: "AC_TimeZone",
                newName: "Holiday3End3");

            migrationBuilder.RenameColumn(
                name: "HolidayEnd2",
                table: "AC_TimeZone",
                newName: "Holiday3End2");

            migrationBuilder.RenameColumn(
                name: "HolidayEnd1",
                table: "AC_TimeZone",
                newName: "Holiday3End1");

            migrationBuilder.AddColumn<string>(
                name: "Holiday1End1",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday1End2",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday1End3",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday1Start1",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday1Start2",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday1Start3",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday2End1",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday2End2",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday2End3",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday2Start1",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday2Start2",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday2Start3",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Holiday1End1",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday1End2",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday1End3",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday1Start1",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday1Start2",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday1Start3",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday2End1",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday2End2",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday2End3",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday2Start1",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday2Start2",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "Holiday2Start3",
                table: "AC_TimeZone");

            migrationBuilder.RenameColumn(
                name: "Holiday3Start3",
                table: "AC_TimeZone",
                newName: "HolidayStart3");

            migrationBuilder.RenameColumn(
                name: "Holiday3Start2",
                table: "AC_TimeZone",
                newName: "HolidayStart2");

            migrationBuilder.RenameColumn(
                name: "Holiday3Start1",
                table: "AC_TimeZone",
                newName: "HolidayStart1");

            migrationBuilder.RenameColumn(
                name: "Holiday3End3",
                table: "AC_TimeZone",
                newName: "HolidayEnd3");

            migrationBuilder.RenameColumn(
                name: "Holiday3End2",
                table: "AC_TimeZone",
                newName: "HolidayEnd2");

            migrationBuilder.RenameColumn(
                name: "Holiday3End1",
                table: "AC_TimeZone",
                newName: "HolidayEnd1");
        }
    }
}
