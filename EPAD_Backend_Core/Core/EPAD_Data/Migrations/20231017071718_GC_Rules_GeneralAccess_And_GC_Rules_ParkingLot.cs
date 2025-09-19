using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class GC_Rules_GeneralAccess_And_GC_Rules_ParkingLot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_Rules_GeneralAccess",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameInEng = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CheckInByShift = table.Column<bool>(type: "bit", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    MaxEarlyCheckInMinute = table.Column<int>(type: "int", nullable: false),
                    MaxLateCheckInMinute = table.Column<int>(type: "int", nullable: false),
                    CheckOutByShift = table.Column<bool>(type: "bit", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    MaxEarlyCheckOutMinute = table.Column<int>(type: "int", nullable: false),
                    MaxLateCheckOutMinute = table.Column<int>(type: "int", nullable: false),
                    AdjustByLateInEarlyOut = table.Column<bool>(type: "bit", nullable: false),
                    AllowInLeaveDay = table.Column<bool>(type: "bit", nullable: false),
                    AllowInMission = table.Column<bool>(type: "bit", nullable: false),
                    AllowInBreakTime = table.Column<bool>(type: "bit", nullable: false),
                    AllowCheckOutInWorkingTime = table.Column<bool>(type: "bit", nullable: false),
                    MaxMinuteAllowOutsideInWorkingTime = table.Column<int>(type: "int", nullable: false),
                    DenyInLeaveWholeDay = table.Column<bool>(type: "bit", nullable: false),
                    DenyInMissionWholeDay = table.Column<bool>(type: "bit", nullable: false),
                    DenyInStoppedWorkingInfo = table.Column<bool>(type: "bit", nullable: false),
                    CheckLogByAreaGroup = table.Column<bool>(type: "bit", nullable: false),
                    CheckLogByShift = table.Column<bool>(type: "bit", nullable: false),
                    UseDefault = table.Column<bool>(type: "bit", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_GeneralAccess", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_ParkingLot",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameInEng = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UseTimeLimitParking = table.Column<bool>(type: "bit", nullable: true),
                    LimitDayNumber = table.Column<int>(type: "int", nullable: true),
                    UseCardDependent = table.Column<bool>(type: "bit", nullable: true),
                    UseRequiredParkingLotAccessed = table.Column<bool>(type: "bit", nullable: true),
                    UseRequiredEmployeeVehicle = table.Column<bool>(type: "bit", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_ParkingLot", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Rules_GeneralAccess");

            migrationBuilder.DropTable(
                name: "GC_Rules_ParkingLot");
        }
    }
}
