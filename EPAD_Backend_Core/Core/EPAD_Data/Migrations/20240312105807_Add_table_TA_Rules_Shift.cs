using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_TA_Rules_Shift : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TA_Rules_Shift",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleInOut = table.Column<int>(type: "int", nullable: true),
                    EarliestAttendanceRangeTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LatestAttendanceRangeTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutOvernightTime = table.Column<bool>(type: "bit", nullable: false),
                    AllowedDoNotAttendance = table.Column<bool>(type: "bit", nullable: false),
                    MissingCheckInAttendanceLogIs = table.Column<int>(type: "int", nullable: true),
                    MissingCheckOutAttendanceLogIs = table.Column<int>(type: "int", nullable: true),
                    LateCheckInMinutes = table.Column<int>(type: "int", nullable: true),
                    EarlyCheckOutMinutes = table.Column<int>(type: "int", nullable: true),
                    MaximumAnnualLeaveRegisterByMonth = table.Column<int>(type: "int", nullable: true),
                    MaximumAnnualLeaveRegisterByYear = table.Column<int>(type: "int", nullable: true),
                    RoundingWorkedTime = table.Column<bool>(type: "bit", nullable: false),
                    RoundingWorkedTimeNum = table.Column<int>(type: "int", nullable: true),
                    RoundingWorkedTimeType = table.Column<int>(type: "int", nullable: true),
                    RoundingOTTime = table.Column<bool>(type: "bit", nullable: false),
                    RoundingOTTimeNum = table.Column<int>(type: "int", nullable: true),
                    RoundingOTTimeType = table.Column<int>(type: "int", nullable: true),
                    RoundingWorkedHour = table.Column<bool>(type: "bit", nullable: false),
                    RoundingWorkedHourNum = table.Column<int>(type: "int", nullable: true),
                    RoundingWorkedHourType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_Rules_Shift", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TA_Rules_Shift");
        }
    }
}
