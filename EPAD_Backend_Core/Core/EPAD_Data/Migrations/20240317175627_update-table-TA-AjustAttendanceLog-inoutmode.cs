using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class updatetableTAAjustAttendanceLoginoutmode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "InOutMode",
                table: "TA_AjustAttendanceLog",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "TA_AjustAttendanceLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "VerifyMode",
                table: "TA_AjustAttendanceLog",
                type: "smallint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InOutMode",
                table: "TA_AjustAttendanceLog");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "TA_AjustAttendanceLog");

            migrationBuilder.DropColumn(
                name: "VerifyMode",
                table: "TA_AjustAttendanceLog");
        }
    }
}
