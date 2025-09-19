using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EPAD_Data.Migrations.Sync_
{
    public partial class addintegratetable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IC_BussinessTravel_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodeAttendance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeEmp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessTravelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessTripType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_BussinessTravel_Integrate_AVN", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Department_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrgStructureName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusFormat = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Department_Integrate_AVN", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Employee_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeEmp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeAttendance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrgStructureCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusSyn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateQuit = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    datecreate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Flag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Employee_Integrate_AVN", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_EmployeeShift_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodeEmp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_EmployeeShift_Integrate_AVN", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_OverTimePlan_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodeEmp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeAttendance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkDateRoot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_OverTimePlan_Integrate_AVN", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Position_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionEngName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Position_Integrate_AVN", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "IC_Shift_Integrate_AVN",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CoOut = table.Column<double>(type: "float", nullable: false),
                    OutTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IC_Shift_Integrate_AVN", x => x.Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IC_BussinessTravel_Integrate_AVN");

            migrationBuilder.DropTable(
                name: "IC_Department_Integrate_AVN");

            migrationBuilder.DropTable(
                name: "IC_Employee_Integrate_AVN");

            migrationBuilder.DropTable(
                name: "IC_EmployeeShift_Integrate_AVN");

            migrationBuilder.DropTable(
                name: "IC_OverTimePlan_Integrate_AVN");

            migrationBuilder.DropTable(
                name: "IC_Position_Integrate_AVN");

            migrationBuilder.DropTable(
                name: "IC_Shift_Integrate_AVN");
        }
    }
}
