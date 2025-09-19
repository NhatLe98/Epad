using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updateaccgroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone1",
                table: "AC_AccGroup");

            migrationBuilder.DropColumn(
                name: "Timezone2",
                table: "AC_AccGroup");

            migrationBuilder.DropColumn(
                name: "Timezone3",
                table: "AC_AccGroup");

            migrationBuilder.RenameColumn(
                name: "TimeZone",
                table: "AC_AccGroup",
                newName: "Timezone");

            migrationBuilder.AddColumn<int>(
                name: "Timezone",
                table: "AC_Door",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimezoneRange",
                table: "AC_AccHoliday",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Timezone",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneString",
                table: "AC_AccGroup",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "AC_Door");

            migrationBuilder.DropColumn(
                name: "TimezoneRange",
                table: "AC_AccHoliday");

            migrationBuilder.DropColumn(
                name: "TimeZoneString",
                table: "AC_AccGroup");

            migrationBuilder.RenameColumn(
                name: "Timezone",
                table: "AC_AccGroup",
                newName: "TimeZone");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "AC_AccGroup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Timezone1",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Timezone2",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Timezone3",
                table: "AC_AccGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
