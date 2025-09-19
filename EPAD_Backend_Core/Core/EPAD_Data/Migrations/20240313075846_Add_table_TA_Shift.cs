using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class Add_table_TA_Shift : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TA_Shift",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((2))"),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RulesShiftIndex = table.Column<int>(type: "int", nullable: false),
                    IsPaidHolidayShift = table.Column<bool>(type: "bit", nullable: false),
                    PaidHolidayStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidHolidayEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidHolidayEndOvernightTime = table.Column<bool>(type: "bit", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutOvernightTime = table.Column<bool>(type: "bit", nullable: false),
                    IsBreakTime = table.Column<bool>(type: "bit", nullable: false),
                    BreakStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakEndOvernightTime = table.Column<bool>(type: "bit", nullable: false),
                    DetermineBreakTimeByAttendanceLog = table.Column<bool>(type: "bit", nullable: false),
                    IsOT = table.Column<bool>(type: "bit", nullable: false),
                    OTStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OTEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OTEndOvernightTime = table.Column<bool>(type: "bit", nullable: false),
                    AllowLateInMinutes = table.Column<int>(type: "int", nullable: true),
                    AllowEarlyOutMinutes = table.Column<int>(type: "int", nullable: true),
                    TheoryWorkedTimeByShift = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA_Shift", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TA_Shift");
        }
    }
}
