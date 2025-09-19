using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class TA_TimeAttendanceLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TA_TimeAttendanceLog",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeATID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftIndex = table.Column<int>(type: "int", nullable: true),
                    CheckIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWorkingDay = table.Column<double>(type: "float", nullable: false),
                    TotalWorkingTime = table.Column<double>(type: "float", nullable: false),
                    TotalDayOff = table.Column<double>(type: "float", nullable: false),
                    TotalHoliday = table.Column<double>(type: "float", nullable: false),
                    TotalOverTime = table.Column<double>(type: "float", nullable: false),
                    TotalBusinessTrip = table.Column<double>(type: "float", nullable: false),
                    TotalWorkingTimeNormal = table.Column<double>(type: "float", nullable: false),
                    TotalOverTimeNormal = table.Column<double>(type: "float", nullable: false),
                    TotalOverTimeNightNormal = table.Column<double>(type: "float", nullable: false),
                    TotalOverTimeDayOff = table.Column<double>(type: "float", nullable: false),
                    TotalOverTimeNightDayOff = table.Column<double>(type: "float", nullable: false),
                    TotalOverTimeHoliday = table.Column<double>(type: "float", nullable: false),
                    TotalOverTimeNightHoliday = table.Column<double>(type: "float", nullable: false),
                    CheckInLate = table.Column<double>(type: "float", nullable: false),
                    CheckOutEarly = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_TimeAttendanceLog", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TA_TimeAttendanceLog");
        }
    }
}
