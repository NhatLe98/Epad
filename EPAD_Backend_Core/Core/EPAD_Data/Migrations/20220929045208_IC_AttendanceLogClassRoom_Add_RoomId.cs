using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class IC_AttendanceLogClassRoom_Add_RoomId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "IC_AttendanceLogClassRoom",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "IC_AttendanceLogClassRoom",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomCode",
                table: "IC_AttendanceLog",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "IC_AttendanceLogClassRoom");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "IC_AttendanceLogClassRoom");

            migrationBuilder.DropColumn(
                name: "RoomCode",
                table: "IC_AttendanceLog");
        }
    }
}
