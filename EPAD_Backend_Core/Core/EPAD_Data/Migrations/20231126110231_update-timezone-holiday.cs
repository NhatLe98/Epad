using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatetimezoneholiday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HolidayEnd1",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolidayEnd2",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolidayEnd3",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolidayStart1",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolidayStart2",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolidayStart3",
                table: "AC_TimeZone",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolidayEnd1",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "HolidayEnd2",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "HolidayEnd3",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "HolidayStart1",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "HolidayStart2",
                table: "AC_TimeZone");

            migrationBuilder.DropColumn(
                name: "HolidayStart3",
                table: "AC_TimeZone");
        }
    }
}
