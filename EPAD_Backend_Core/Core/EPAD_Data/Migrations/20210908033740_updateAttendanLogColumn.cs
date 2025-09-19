using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updateAttendanLogColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BodyBodyTemperature",
                table: "IC_AttendanceLog",
                newName: "BodyTemperature");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BodyTemperature",
                table: "IC_AttendanceLog",
                newName: "BodyBodyTemperature");
        }
    }
}
