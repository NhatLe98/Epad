using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class add2columnsAddtandenLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BodyBodyTemperature",
                table: "IC_AttendanceLog",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceMask",
                table: "IC_AttendanceLog",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "BodyBodyTemperature",
                table: "IC_AttendanceLog");

            migrationBuilder.DropColumn(
                name: "FaceMask",
                table: "IC_AttendanceLog");
        }
    }
}
