using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Backend_Core.Migrations
{
    public partial class addrulegeneral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GC_Rules_General",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameInEng = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartTimeDay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaxAttendanceTime = table.Column<int>(type: "int", nullable: false),
                    IsUsing = table.Column<bool>(type: "bit", nullable: false),
                    IsBypassRule = table.Column<bool>(type: "bit", nullable: true),
                    PresenceTrackingTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_General", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_Rules_General_Log",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaGroupIndex = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UseDeviceMode = table.Column<bool>(type: "bit", nullable: false),
                    UseSequenceLog = table.Column<bool>(type: "bit", nullable: false),
                    UseMinimumLog = table.Column<bool>(type: "bit", nullable: false),
                    UseTimeLog = table.Column<bool>(type: "bit", nullable: false),
                    UseMode = table.Column<int>(type: "int", nullable: false),
                    MinimumLog = table.Column<int>(type: "int", nullable: false),
                    FromEarlyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToLateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FromIsNextDay = table.Column<bool>(type: "bit", nullable: false),
                    ToIsNextDay = table.Column<bool>(type: "bit", nullable: false),
                    ToLateIsNextDay = table.Column<bool>(type: "bit", nullable: false),
                    RuleGeneralIndex = table.Column<int>(type: "int", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_Rules_General_Log", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "GC_TimeLog",
                columns: table => new
                {
                    EmployeeATID = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    MachineSerial = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    InOutMode = table.Column<short>(type: "smallint", nullable: true),
                    SpecifiedMode = table.Column<short>(type: "smallint", nullable: true),
                    Action = table.Column<string>(type: "varchar(5)", nullable: true),
                    SystemTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtendData = table.Column<string>(type: "ntext", nullable: true),
                    ObjectAccessType = table.Column<string>(type: "varchar(10)", nullable: true),
                    CustomerIndex = table.Column<int>(type: "int", nullable: false),
                    LogType = table.Column<string>(type: "varchar(10)", nullable: true),
                    PlatesRegistered = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    LineIndex = table.Column<int>(type: "int", nullable: false),
                    GateIndex = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApproveStatus = table.Column<short>(type: "smallint", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifyMode = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_TimeLog", x => new { x.EmployeeATID, x.CompanyIndex, x.Time, x.MachineSerial });
                });

            migrationBuilder.CreateTable(
                name: "GC_TimeLog_Image",
                columns: table => new
                {
                    Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeLogIndex = table.Column<long>(type: "bigint", nullable: false),
                    Image1 = table.Column<string>(type: "text", nullable: true),
                    Image2 = table.Column<string>(type: "text", nullable: true),
                    Image3 = table.Column<string>(type: "text", nullable: true),
                    Image4 = table.Column<string>(type: "text", nullable: true),
                    Image5 = table.Column<string>(type: "text", nullable: true),
                    Info1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Info2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Info3 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Info4 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Info5 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyIndex = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GC_TimeLog_Image", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GC_Rules_General");

            migrationBuilder.DropTable(
                name: "GC_Rules_General_Log");

            migrationBuilder.DropTable(
                name: "GC_TimeLog");

            migrationBuilder.DropTable(
                name: "GC_TimeLog_Image");
        }
    }
}
