using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class TA_Rules_Global_Add_New_Field_AutoCalculateAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAutoCalculateAttendance",
                table: "TA_Rules_Global",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimePos",
                table: "TA_Rules_Global",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutoCalculateAttendance",
                table: "TA_Rules_Global");

            migrationBuilder.DropColumn(
                name: "TimePos",
                table: "TA_Rules_Global");
        }
    }
}
