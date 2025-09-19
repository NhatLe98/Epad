using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class TA_TimeAttendanceLog_AddTotalWorkingTimeNight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalWorkingTimeNight",
                table: "TA_TimeAttendanceLog",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalWorkingTimeNight",
                table: "TA_TimeAttendanceLog");
        }
    }
}
